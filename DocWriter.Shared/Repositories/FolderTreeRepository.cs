using DocWriter.Shared.Models;

namespace DocWriter.Shared.Repositories;

public class FolderTreeRepository : IFolderTreeRepository
{
    public async Task<IReadOnlyCollection<FolderTreeItem>> GetFolderTreeItemsAsync(FolderTreeItem parent)
    {
        await Task.Delay(300);

        return DataStorageMock.FolderTreeMock.Where(x => string.Join('/', x.Path.Split('/')[0..^1]) == parent.Path).ToList();
    }

    public Task<FolderTreeItem> CreateFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();

    public async Task<FolderTreeItem> UpdateFolderTreeItemAsync(RenameFolderTreeItem item)
    {
        var original = DataStorageMock.FolderTreeMock.Single(x => x.Path == item.Path);

        original = original with { Name = item.NewName, Path = original.Path.Replace(original.Name, item.NewName) };
        await Task.Delay(300);

        return original;
    }

    public Task<bool> DeleteFolderTreeItemAsync(FolderTreeItem item)
    {
        DataStorageMock.FolderTreeMock.Remove(item);
        return Task.FromResult(true);
    }
}