using DocWriter.Shared.Models;

namespace DocWriter.Shared;

public interface IFolderTreeRepository
{
    Task<IReadOnlyCollection<FolderTreeItem>> GetFolderTreeItemsAsync(FolderTreeItem parent);

    Task<FolderTreeItem> CreateFolderTreeItemAsync(FolderTreeItem item);

    Task<FolderTreeItem> UpdateFolderTreeItemAsync(FolderTreeItem item);

    Task DeleteFolderTreeItemAsync(FolderTreeItem item);
}