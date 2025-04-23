using DocWriter.Shared.Models;
using MudBlazor;

namespace DocWriter.Client.StateContainers;

internal class HomeStateContainer : IHomeStateContainer
{
    public List<TreeItemData<FolderTreeItem>> TreeItemData { get; } = new();

    public Dictionary<string, TreeItemData<FolderTreeItem>> ItemsMap { get; } = [];

    public FolderTreeItem SelectedItem { get; set; }

    public bool IsDrawerOpen { get; set; } = true;

    public bool IsFullScreen { get; set; }

    public bool EditMode { get; set; }

    public string Visibility { get; set; } = "visible";

    public double TreeWidth { get; set; }
}