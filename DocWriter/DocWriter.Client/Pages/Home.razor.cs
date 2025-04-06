using DocWriter.Shared;
using DocWriter.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using MudBlazor;

namespace DocWriter.Client.Pages;

public partial class Home
{
    private string _markdownValue;

    private string _visibility = "visible";
    private bool _isFullScreen = false;
    private List<TreeItemData<FolderTreeItem>> _treeItemData = [];
    private MudTreeView<FolderTreeItem> _treeView;
    private string _searchPhrase;
    private bool _isDrawerOpen = true;

    private string HtmlContent { get; set; }

    [Inject]
    private IJSRuntime JsRuntime { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; }

    [Inject]
    private IEditorFullScreenModeValueHolder EditorFullScreenModeValueHolder { get; set; }

    [Inject]
    private IFolderTreeRepository FolderTreeRepository { get; init; }

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

    protected override async Task OnInitializedAsync()
    {
        _markdownValue =
            "```mermaid\nstateDiagram\n    [*] --> Still\n    Still --> [*]\n\n    Still --> Moving\n    Moving --> Still\n    Moving --> Crash\n    Crash --> [*]\n```";
        NavigationManager.LocationChanged += NavigationManagerOnLocationChanged;
        NavigateToElement();

        var folderTreeItems = await FolderTreeRepository.GetFolderTreeItemsAsync(null);

        _treeItemData.AddRange(
            folderTreeItems.Select(
                x => new TreeItemData<FolderTreeItem>
                {
                    Icon = GetProperIcon(x),
                    Text = x.Name,
                    Children = x.Children?.Select(
                                y => new TreeItemData<FolderTreeItem>
                                {
                                    Icon = GetProperIcon(y),
                                    Text = y.Name
                                })
                            .ToList()
                        ?? []
                }));

        EditorFullScreenModeValueHolder.IsFullScreenChanged += (sender, b) =>
        {
            _visibility = b ? "hidden" : "visible";
            StateHasChanged();
        };
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        IJSObjectReference mermaidModule =
            await JsRuntime.InvokeAsync<IJSObjectReference>("import", "./modules/mermaidmodule.js");

        if (firstRender)
        {
            await mermaidModule.InvokeVoidAsync("Initialize");
            await mermaidModule.InvokeVoidAsync("updateFullScreenState", DotNetObjectReference.Create(this));

            // await _mermaidModule.InvokeVoidAsync("Render", "mermaid");
        }
        else
        {
            // await mermaidModule.InvokeVoidAsync("RenderAll");
        }
    }

    private void NavigationManagerOnLocationChanged(object sender, LocationChangedEventArgs e)
    {
        NavigateToElement();
    }

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

    private Task OnMarkdownValueChangedAsync(string value)
    {
        _markdownValue = value;

        return Task.CompletedTask;
    }

    private Task OnMarkdownValueHTMLChanged(string value)
    {
        HtmlContent = value;

        return Task.CompletedTask;
    }

    private void ScrollToElementId(object elementId)
    {
        JsRuntime.InvokeVoidAsync("scrollToElementId", elementId).GetAwaiter().GetResult();
    }

    private async void OnTextChanged(string searchPhrase)
    {
        _searchPhrase = searchPhrase;
        await _treeView.FilterAsync();
    }

    private Task<bool> MatchesName(TreeItemData<FolderTreeItem> item)
    {
        if (string.IsNullOrEmpty(item.Text))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(item.Text.Contains(_searchPhrase, StringComparison.OrdinalIgnoreCase));
    }

    private string GetProperIcon(FolderTreeItem item)
    {
        return item.Type switch
        {
            FolderTreeItemType.Folder => Icons.Material.Filled.Folder,
            FolderTreeItemType.Markdown => Icons.Custom.FileFormats.FileDocument,
            FolderTreeItemType.Image => Icons.Custom.FileFormats.FileImage,
            _ => Icons.Material.Filled.QuestionAnswer
        };
    }

    private void ToggleDrawer()
    {
        _isDrawerOpen = !_isDrawerOpen;
    }
}