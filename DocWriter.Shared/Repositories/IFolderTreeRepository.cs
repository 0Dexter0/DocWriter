using DocWriter.Shared.Models;

namespace DocWriter.Shared.Repositories;

public interface IFolderTreeRepository
{
    Task<IReadOnlyCollection<FolderTreeItem>> GetFolderTreeItemsAsync(FolderTreeItem parent);

    Task<FolderTreeItem> CreateFolderTreeItemAsync(FolderTreeItem item);

    Task<FolderTreeItem> UpdateFolderTreeItemAsync(RenameFolderTreeItem item);

    Task<bool> DeleteFolderTreeItemAsync(FolderTreeItem item);
}