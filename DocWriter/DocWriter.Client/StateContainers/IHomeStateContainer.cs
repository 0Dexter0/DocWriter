using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Client.StateContainers;

internal interface IHomeStateContainer
{
    List<TreeItemData<FolderTreeItem>> TreeItemData { get; }

    Dictionary<string, TreeItemData<FolderTreeItem>> ItemsMap { get; }

    FolderTreeItem SelectedItem { get; set; }

    bool IsDrawerOpen { get; set; }

    bool IsFullScreen { get; set; }

    bool EditMode { get; set; }

    string Visibility { get; set; }

    double TreeWidth { get; set; }
}