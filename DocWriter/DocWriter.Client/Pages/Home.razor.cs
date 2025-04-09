using DocWriter.Shared;
using DocWriter.Shared.Models;
using DocWriter.Shared.Providers;
using DocWriter.Shared.Repositories;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace DocWriter.Client.Pages;

public partial class Home : IDisposable
{
    private string _markdownValue;
    private bool _isFullScreen;
    private bool _editMode;
    private bool _disposed;
    private string _searchPhrase;
    private string _editFilePath;
    private FolderTreeItem _selectedItem;
    private MudTreeView<FolderTreeItem> _treeView;
    private string _visibility = "visible";
    private bool _isDrawerOpen = true;
    private readonly List<TreeItemData<FolderTreeItem>> _treeItemData = [];

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IEditorFullScreenModeValueHolder EditorFullScreenModeValueHolder { get; set; }

    [Inject]
    private ITreeItemsProvider TreeItemsProvider { get; init; }

    [Inject]
    private IFileContentRepository FileContentRepository { get; init; }

    [JSInvokable]
    public void StateHasChangedFromJs()
    {
        StateHasChanged();
    }

    [JSInvokable]
    public void ToggleFullScreen()
    {
        EditorFullScreenModeValueHolder.InvokeFullScreenChanged(!_isFullScreen);
        _isFullScreen = !_isFullScreen;
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

        var folderTreeItems = await TreeItemsProvider.GetTreeItemsAsync(
            new(string.Empty, string.Empty, FolderTreeItemType.Folder));
        _treeItemData.AddRange(folderTreeItems);

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
        }

        _disposed = true;
    }

    private Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> ServerData(FolderTreeItem parent) =>
        TreeItemsProvider.GetTreeItemsAsync(parent);

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
        _visibility = b ? "hidden" : "visible";
        StateHasChanged();
    }

    private Task OnTextChangedAsync(string searchPhrase)
    {
        _searchPhrase = searchPhrase;
        return _treeView.FilterAsync();
    }

    private Task<bool> MatchesNameAsync(TreeItemData<FolderTreeItem> item)
    {
        if (string.IsNullOrEmpty(item.Text))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(item.Text.Contains(_searchPhrase, StringComparison.OrdinalIgnoreCase));
    }

    private void ToggleDrawer()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }

    private async Task EditItemAsync(string filePath)
    {
        _editMode = true;
        _editFilePath = filePath;

        string fileContent = await FileContentRepository.GetFileContentAsync(filePath);
        _markdownValue = fileContent;

        var module =
            await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");
        await module.InvokeVoidAsync("editorStateChangingHandler", DotNetObjectReference.Create(this));
    }

    private void DeleteItem()
    {
        // TODO: implement
    }

    private void CancelEdit() => _editMode = false;

    private async Task SaveAsync()
    {
        await FileContentRepository.UpdateFileContentAsync(_editFilePath, _markdownValue);
        _editMode = false;
        StateHasChanged();
    }
}