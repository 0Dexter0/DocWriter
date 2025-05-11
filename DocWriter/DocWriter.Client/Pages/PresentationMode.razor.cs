using DocWriter.Client.StateContainers;
using DocWriter.Shared.Models;
using DocWriter.Shared.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace DocWriter.Client.Pages;

public partial class PresentationMode : ComponentBase, IAsyncDisposable
{
    private const string DefaultTitle = "Presentation Mode";

    private bool _isDrawerOpen;
    private bool _isPresentationModeActive;
    private FolderTreeItem _selectedItem;
    private IJSObjectReference _pageModule;

    private string _title = DefaultTitle;
    private string _currentFilePath = String.Empty;

    private readonly List<string> _files = [];
    private readonly List<(string Reference, string Title)> _anchors = [];

    [Inject]
    private ITreeItemsService TreeItemsProvider { get; init; }

    [Inject]
    private IPresentationStateContainer StateContainer { get; init; }

    [Inject]
    private IJSRuntime JsRuntime { get; init; }

    [Inject]
    private NavigationManager NavigationManager { get; init; }

    public async ValueTask DisposeAsync()
    {
        if (_pageModule != null)
        {
            await _pageModule.DisposeAsync();
        }

        NavigationManager.LocationChanged -= NavigationManagerOnLocationChanged;
    }

    [JSInvokable]
    public void AddAnchor(string anchor, string title)
    {
        _anchors.Add((anchor, title));

        if (!_isDrawerOpen)
        {
            _isDrawerOpen = true;
        }

        StateHasChanged();
    }

    protected override async Task OnInitializedAsync()
    {
        _pageModule = await JsRuntime.InvokeAsync<IJSObjectReference>("import", "/PresentationMode.js");
        if (!StateContainer.TreeItemData.Any())
        {
            StateContainer.TreeItemData.AddRange(
                await TreeItemsProvider.GetTreeItemsAsync(new(string.Empty, string.Empty, FolderTreeItemType.Folder)));
        }

        if (NavigationManager.Uri.Contains('#'))
        {
            NavigationManager.NavigateTo(NavigationManager.Uri.Split('#')[0]);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // if (_isPresentationModeActive && !_isHandlerCalled)
        if (_isPresentationModeActive)
        {
            await _pageModule.InvokeVoidAsync("previewHandler", DotNetObjectReference.Create(this));
            // _isHandlerCalled = true;
        }
    }

    private void ToggleDrawer()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }

    private Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> LoadServerData(FolderTreeItem parent) =>
        TreeItemsProvider.GetTreeItemsAsync(parent);

    private void OnItemsLoaded(
        TreeItemData<FolderTreeItem> treeItemData,
        IReadOnlyCollection<TreeItemData<FolderTreeItem>> children) => treeItemData.Children = children?.ToList();

    private void StartPresentation(string filePath)
    {
        if (!IsSelectedItemInProject(out var name, out var projectFolder))
        {
            return;
        }

        foreach (var file in CollectFilesInProject(projectFolder))
        {
            _files.Add(file);
        }

        if (!_files.Any())
        {
            return;
        }

        _title = name;
        _isPresentationModeActive = true;
        _currentFilePath = _files.Single(x => x == filePath);
    }

    private void EndPresentation()
    {
        _isPresentationModeActive = false;
        _isDrawerOpen = false;
        _title = DefaultTitle;
        _files.Clear();
        _anchors.Clear();

        NavigationManager.NavigateTo(NavigationManager.Uri);
    }

    private bool IsSelectedItemInProject(out string name, out TreeItemData<FolderTreeItem> projectFolder)
    {
        string[] selectedItemPathSections = _selectedItem?.Path.Split('/') ?? [];

        if (!selectedItemPathSections.Any())
        {
            name = null;
            projectFolder = null;

            return false;
        }

        string projectRootPath = selectedItemPathSections[0];

        var folder = StateContainer.TreeItemData.Single(x => x.Value!.Path == projectRootPath);

        if (folder.Value!.ProjectRoot)
        {
            name = folder.Text;
            projectFolder = folder;

            return true;
        }

        foreach (var section in selectedItemPathSections.Skip(1))
        {
            projectRootPath += "/" + section;

            folder = StateContainer.TreeItemData.Single(x => x.Value!.Path == projectRootPath);

            if (folder.Value!.ProjectRoot)
            {
                name = folder.Text;
                projectFolder = folder;

                return true;
            }
        }

        name = null;
        projectFolder = null;

        return false;
    }

    private string[] CollectFilesInProject(TreeItemData<FolderTreeItem> projectFolder)
    {
        var files = GetFiles(projectFolder);

        return files.Select(x => x.Path).ToArray();
    }

    private FolderTreeItem[] GetFiles(TreeItemData<FolderTreeItem> folder)
    {
        List<FolderTreeItem> files = [];

        files.AddRange(
            folder.Children?.Where(x => x.Value!.Type == FolderTreeItemType.Markdown)
                .Select(x => x.Value)
            ?? []);
        files.AddRange(
            folder.Children?.Where(x => x.Value!.Type == FolderTreeItemType.Folder)
                .SelectMany(GetFiles) ?? []);

        return files.ToArray();
    }

    private void GoPrevious()
    {
        _anchors.Clear();
        _isDrawerOpen = false;
        _currentFilePath = _files[_files.IndexOf(_currentFilePath) - 1];
    }

    private void GoNext()
    {
        _anchors.Clear();
        _isDrawerOpen = false;
        _currentFilePath = _files[_files.IndexOf(_currentFilePath) + 1];
    }

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

    private void ScrollToElementId(object elementId) => JsRuntime.InvokeVoidAsync("scrollToElementId", elementId).GetAwaiter().GetResult();
}