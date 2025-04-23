using DocWriter.Client.Components;
using DocWriter.Client.StateContainers;
using DocWriter.Shared;
using DocWriter.Shared.Models;
using DocWriter.Shared.Repositories;
using DocWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace DocWriter.Client.Pages;

public partial class Home : IDisposable
{
    private const double MaxTreeWidth = 600;
    private string _markdownValue;
    private bool _disposed;
    private string _editFilePath;
    private bool _isResizing;
    private double _resizeStartX;

    [Inject]
    private IJSRuntime JsRuntime { get; init; }

    [Inject]
    private NavigationManager NavigationManager { get; init; }

    [Inject]
    private IEditorFullScreenModeValueHolder EditorFullScreenModeValueHolder { get; init; }

    [Inject]
    private ITreeItemsService TreeItemsService { get; init; }

    [Inject]
    private IFileContentRepository FileContentRepository { get; init; }

    [Inject]
    private IDialogService DialogService { get; init; }

    [Inject]
    private ISnackbar Snackbar { get; init; }

    [Inject]
    private IHomeStateContainer HomeStateContainer { get; init; }

    [JSInvokable]
    public void StateHasChangedFromJs()
    {
        StateHasChanged();
    }

    [JSInvokable]
    public void ToggleFullScreen()
    {
        EditorFullScreenModeValueHolder.InvokeFullScreenChanged(!HomeStateContainer.IsFullScreen);
        HomeStateContainer.IsFullScreen = !HomeStateContainer.IsFullScreen;
        StateHasChanged();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
        NavigateToElement();

        if (!HomeStateContainer.TreeItemData.Any())
        {
            TreeItemData<FolderTreeItem> root = new()
            {
                Icon = Icons.Material.Filled.Folder,
                Expandable = false,
                Text = "Root",
                Value = new("Root", String.Empty, FolderTreeItemType.Folder)
            };

            var folderTreeItems = await TreeItemsService.GetTreeItemsAsync(
                new(string.Empty, string.Empty, FolderTreeItemType.Folder));
            HomeStateContainer.TreeItemData.Add(root);
            HomeStateContainer.ItemsMap[String.Empty] = root;

            if (folderTreeItems.Any())
            {
                root.Children = folderTreeItems.ToList();
                root.Expandable = true;
                root.Expanded = true;

                foreach (var item in folderTreeItems)
                {
                    HomeStateContainer.ItemsMap[item.Value!.Path] = item;
                }

                StateHasChanged();
            }
        }

        EditorFullScreenModeValueHolder.IsFullScreenChanged += FullScreenVisibilityHandler;
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IJSObjectReference mermaidModule =
            await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");

        if (firstRender)
        {
            await mermaidModule.InvokeVoidAsync("Initialize");
            await mermaidModule.InvokeVoidAsync("editorStateChangingHandler", DotNetObjectReference.Create(this));
        }
        else
        {
            await mermaidModule.InvokeVoidAsync("RenderAll");
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            EditorFullScreenModeValueHolder.IsFullScreenChanged -= FullScreenVisibilityHandler;
            NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
            HomeStateContainer.EditMode = false;
        }

        _disposed = true;
    }

    private async Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> ServerData(FolderTreeItem parent)
    {
        var items = await TreeItemsService.GetTreeItemsAsync(parent);

        foreach (var item in items)
        {
            HomeStateContainer.ItemsMap[item.Value!.Path] = item;
        }

        return items;
    }

    private void OnItemsLoaded(TreeItemData<FolderTreeItem> treeItemData, IReadOnlyCollection<TreeItemData<FolderTreeItem>> children) =>
        treeItemData.Children = children?.ToList();

    private void NavigationManagerOnLocationChanged(object sender, LocationChangedEventArgs e) => NavigateToElement();

    private void NavigateToElement()
    {
        var url = NavigationManager.Uri;
        var fragment = new Uri(url).Fragment;

        if (string.IsNullOrEmpty(fragment))
        {
            return;
        }

        var elementId = fragment.StartsWith("#") ? fragment.Substring(1) : fragment;

        if (string.IsNullOrEmpty(elementId))
        {
            return;
        }

        ScrollToElementId(elementId);
    }

    private void OnMarkdownValueChanged(string value) => _markdownValue = value;

    private void ScrollToElementId(object elementId) => JsRuntime.InvokeVoidAsync("scrollToElementId", elementId).GetAwaiter().GetResult();

    private void FullScreenVisibilityHandler(object _, bool b)
    {
        HomeStateContainer.Visibility = b ? "hidden" : "visible";
        StateHasChanged();
    }

    private void ToggleDrawer() => HomeStateContainer.IsDrawerOpen = !HomeStateContainer.IsDrawerOpen;

    private async Task EditItemAsync(string filePath)
    {
        HomeStateContainer.EditMode = true;
        _editFilePath = filePath;

        string fileContent = await FileContentRepository.GetFileContentAsync(filePath);
        _markdownValue = fileContent;
        StateHasChanged();

        var module =
            await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");
        await module.InvokeVoidAsync("editorStateChangingHandler", DotNetObjectReference.Create(this));
        StateHasChanged();
    }

    private void CancelEdit()
    {
        HomeStateContainer.EditMode = false;
        _markdownValue = null;
    }

    private async Task SaveAsync()
    {
        await FileContentRepository.UpdateFileContentAsync(_editFilePath, _markdownValue);
        HomeStateContainer.EditMode = false;
        _markdownValue = null;

        StateHasChanged();
    }

    private bool IsItemInProject(string path)
    {
        string[] selectedItemPathSections = path.Split('/');

        if (!selectedItemPathSections.Any())
        {
            return false;
        }

        string projectRootPath = selectedItemPathSections[0];

        var folder = HomeStateContainer.TreeItemData.First().Children!.Single(x => x.Value!.Path == projectRootPath);

        if (folder.Value!.ProjectRoot)
        {
            return true;
        }

        foreach (var section in selectedItemPathSections.Skip(1))
        {
            projectRootPath += "/" + section;

            folder = HomeStateContainer.TreeItemData.Single(x => x.Value!.Path == projectRootPath);

            if (folder.Value!.ProjectRoot)
            {
                return true;
            }
        }

        return false;
    }

    private async Task RenameItemAsync(TreeItemData<FolderTreeItem> item)
    {
        DialogOptions options = new() { CloseOnEscapeKey = true };
        DialogParameters<RenameDialog> parameters = new()
        {
            { "Item", item },
            { "Type", item.Value!.Type is FolderTreeItemType.Folder ? "Folder" : "File" }
        };

        var dialog = await DialogService.ShowAsync<RenameDialog>(item.Value.Name, parameters, options);
        await dialog.Result;
    }

    private async Task DeleteItemAsync(TreeItemData<FolderTreeItem> item)
    {
        var result = await TreeItemsService.DeleteAsync(item.Value!);

        if (!result)
        {
            Snackbar.Add($"Unable to delete {item.Value!.Path}.", Severity.Error);
            return;
        }

        Snackbar.Add("Successfully deleted.", Severity.Success);

        TreeItemData<FolderTreeItem> parent = null;

        if (!item.Value!.Path.Contains('/'))
        {
            parent = HomeStateContainer.TreeItemData.First();
        }
        else if (!HomeStateContainer.ItemsMap.TryGetValue(item.Value!.Path.Substring(0, item.Value.Path.LastIndexOf('/')), out parent))
        {
            return;
        }

        parent.Children!.Remove(item);
        StateHasChanged();
    }
}