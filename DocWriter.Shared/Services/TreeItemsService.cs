using DocWriter.Shared.Models;
using DocWriter.Shared.Repositories;
using Microsoft.Extensions.Caching.Memory;
using MudBlazor;

namespace DocWriter.Shared.Services;

internal class TreeItemsService : ITreeItemsService
{
    // private readonly Dictionary<string, IReadOnlyCollection<FolderTreeItem>> _cache = new();
    private readonly IMemoryCache _cache;

    private readonly IFolderTreeRepository _folderTreeRepository;

    public TreeItemsService(IFolderTreeRepository folderTreeRepository, IMemoryCache cache)
    {
        _folderTreeRepository = folderTreeRepository;
        _cache = cache;
    }

    public async Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> GetTreeItemsAsync(FolderTreeItem parent)
    {
        var items = await GetFromCacheAsync(parent);

        return items.Select(
                x =>
                {
                    var converted = Convert(x);

                    // if (_cache.TryGetValue(x.Path, out var children))
                    // {
                    //     converted.Children = children.Select(Convert).ToList();
                    // }

                    return converted;
                })
            .ToArray();
    }

    public async Task<bool> DeleteAsync(FolderTreeItem item)
    {
        var result = await _folderTreeRepository.DeleteFolderTreeItemAsync(item);

        if (!result)
        {
            return false;
        }

        string parentPath = item.Path.Contains('/') ? item.Path.Substring(0, item.Path.LastIndexOf('/')) : String.Empty;
        _cache.Remove(parentPath);
        _ = GetFromCacheAsync(new(String.Empty, parentPath, FolderTreeItemType.None));

        return true;
    }

    // public async Task RenameAsync(RenameFolderTreeItem item)
    // {
    //     _cache.
    //     string parentPath = item.Path.Substring(0, item.Path.LastIndexOf('/'));
    //     var original = _cache[parentPath].Single(x => x.Name == item.OldName);
    //     var result = await _folderTreeRepository.UpdateFolderTreeItemAsync(item);
    //
    //
    // }

    private TreeItemData<FolderTreeItem> Convert(FolderTreeItem item) => new()
    {
        Icon = GetProperIcon(item),
        Text = item.Name,
        Value = item,
        Expandable = item.Expandable
    };

    private string GetProperIcon(FolderTreeItem item) => item.Type switch
    {
        FolderTreeItemType.Folder => item.ProjectRoot
            ? Icons.Material.Filled.FolderSpecial
            : Icons.Material.Filled.Folder,
        FolderTreeItemType.Markdown => Icons.Custom.FileFormats.FileDocument,
        FolderTreeItemType.Image => Icons.Custom.FileFormats.FileImage,
        _ => Icons.Material.Filled.QuestionAnswer
    };

    private Task<IReadOnlyCollection<FolderTreeItem>> GetFromCacheAsync(FolderTreeItem parent) =>
        _cache.GetOrCreateAsync(
            parent.Path,
            _ => _folderTreeRepository.GetFolderTreeItemsAsync(parent));
}