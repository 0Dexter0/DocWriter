using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Shared.Services;

public interface ITreeItemsService
{
    Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> GetTreeItemsAsync(FolderTreeItem parent);

    Task<bool> DeleteAsync(FolderTreeItem item);
}