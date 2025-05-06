using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Client.StateContainers;

internal class PresentationStateContainer : IPresentationStateContainer
{
    public List<TreeItemData<FolderTreeItem>> TreeItemData { get; } = [];
}