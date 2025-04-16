﻿using DocWriter.Client.Components;
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
    private string _markdownValue;
    private bool _isFullScreen;
    private bool _editMode;
    private bool _disposed;
    private string _editFilePath;
    private FolderTreeItem _selectedItem;
    private string _visibility = "visible";
    private bool _isDrawerOpen = true;
    private readonly List<TreeItemData<FolderTreeItem>> _treeItemData = [];
    private readonly Dictionary<string, TreeItemData<FolderTreeItem>> _itemsMap = [];

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

        TreeItemData<FolderTreeItem> root = new()
        {
            Icon = Icons.Material.Filled.Folder,
            Expandable = false,
            Text = "Root",
            Value = new("Root", String.Empty, FolderTreeItemType.Folder)
        };

        var folderTreeItems = await TreeItemsService.GetTreeItemsAsync(
            new(string.Empty, string.Empty, FolderTreeItemType.Folder));
        _treeItemData.Add(root);
        _itemsMap[String.Empty] = root;

        if (folderTreeItems.Any())
        {
            root.Children = folderTreeItems.ToList();
            root.Expandable = true;
            root.Expanded = true;

            foreach (var item in folderTreeItems)
            {
                _itemsMap[item.Value!.Path] = item;
            }

            StateHasChanged();
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
        }

        _disposed = true;
    }

    private async Task<IReadOnlyCollection<TreeItemData<FolderTreeItem>>> ServerData(FolderTreeItem parent)
    {
        var items = await TreeItemsService.GetTreeItemsAsync(parent);

        foreach (var item in items)
        {
            _itemsMap[item.Value!.Path] = item;
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
        _visibility = b ? "hidden" : "visible";
        StateHasChanged();
    }

    private void ToggleDrawer() => _isDrawerOpen = !_isDrawerOpen;

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

    private void CancelEdit() => _editMode = false;

    private async Task SaveAsync()
    {
        await FileContentRepository.UpdateFileContentAsync(_editFilePath, _markdownValue);
        _editMode = false;
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

        var folder = _treeItemData.First().Children!.Single(x => x.Value!.Path == projectRootPath);

        if (folder.Value!.ProjectRoot)
        {
            return true;
        }

        foreach (var section in selectedItemPathSections.Skip(1))
        {
            projectRootPath += "/" + section;

            folder = _treeItemData.Single(x => x.Value!.Path == projectRootPath);

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
            parent = _treeItemData.First();
        }
        else if (!_itemsMap.TryGetValue(item.Value!.Path.Substring(0, item.Value.Path.LastIndexOf('/')), out parent))
        {
            return;
        }

        parent.Children!.Remove(item);
        StateHasChanged();
    }
}