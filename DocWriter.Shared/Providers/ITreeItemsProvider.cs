using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Shared.Providers;

public interface ITreeItemsProvider
{
    Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> GetTreeItemsAsync(FolderTreeItem parent);
}