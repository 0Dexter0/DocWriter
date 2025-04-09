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

    public Task<FolderTreeItem> UpdateFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();

    public Task DeleteFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();
}