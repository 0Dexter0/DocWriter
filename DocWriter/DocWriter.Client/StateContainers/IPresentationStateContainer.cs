using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Client.StateContainers;

public interface IPresentationStateContainer
{
    List<TreeItemData<FolderTreeItem>> TreeItemData { get; }
}