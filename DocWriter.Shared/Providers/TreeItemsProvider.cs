using DocWriter.Shared.Models;
using DocWriter.Shared.Repositories;
using MudBlazor;

namespace DocWriter.Shared.Providers;

internal class TreeItemsProvider : ITreeItemsProvider
{
    private readonly Dictionary<string, IReadOnlyCollection<FolderTreeItem>> _cache = new();

    private readonly IFolderTreeRepository _folderTreeRepository;

    public TreeItemsProvider(IFolderTreeRepository folderTreeRepository)
    {
        _folderTreeRepository = folderTreeRepository;
    }

    public async Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> GetTreeItemsAsync(FolderTreeItem parent)
    {
        if (_cache.TryGetValue(parent.Path, out var items))
        {
            return items.Select(
                x =>
                {
                    var converted = Convert(x);

                    if (_cache.TryGetValue(x.Path, out var children))
                    {
                        converted.Children = children.Select(Convert).ToList();
                    }

                    return converted;
                }).ToArray();
        }

        var result = await _folderTreeRepository.GetFolderTreeItemsAsync(parent);

        _cache[parent.Path] = result;

        return result.Select(Convert).ToArray();
    }

    private TreeItemData<FolderTreeItem> Convert(FolderTreeItem item) => new()
    {
        Icon = GetProperIcon(item),
        Text = item.Name,
        Value = item,
        Expandable = item.Expandable
    };

    private string GetProperIcon(FolderTreeItem item) => item.Type switch
    {
        FolderTreeItemType.Folder => item.ProjectRoot ? Icons.Material.Filled.FolderSpecial : Icons.Material.Filled.Folder,
        FolderTreeItemType.Markdown => Icons.Custom.FileFormats.FileDocument,
        FolderTreeItemType.Image => Icons.Custom.FileFormats.FileImage,
        _ => Icons.Material.Filled.QuestionAnswer
    };
}