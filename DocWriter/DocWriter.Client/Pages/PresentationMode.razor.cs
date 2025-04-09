using DocWriter.Shared.Models;
using DocWriter.Shared.Providers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace DocWriter.Client.Pages;

public partial class PresentationMode : ComponentBase
{
    private const string DefaultTitle = "Presentation Mode";

    private bool _isDrawerOpen;
    private bool _isPresentationModeActive;
    private FolderTreeItem _selectedItem;

    private string _title = DefaultTitle;
    private string _currentFilePath = String.Empty;
    private readonly List<TreeItemData<FolderTreeItem>> _treeItemData = [];
    private readonly List<string> _files = [];

    [Inject]
    private ITreeItemsProvider TreeItemsProvider { get; init; }

    protected override async Task OnInitializedAsync()
    {
        _treeItemData.AddRange(
            await TreeItemsProvider.GetTreeItemsAsync(new(string.Empty, string.Empty, FolderTreeItemType.Folder)));
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

    private void StartPresentation()
    {
        if (!IsSelectedItemInProject(out var name, out var projectFolder))
        {
            return;
        }

        CollectFilesInProject(projectFolder);

        if (!_files.Any())
        {
            return;
        }

        _title = name;
        _isPresentationModeActive = true;
        _currentFilePath = _files[0];
    }

    private void EndPresentation()
    {
        _isPresentationModeActive = false;
        _title = DefaultTitle;
        _files.Clear();
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

        var folder = _treeItemData.Single(x => x.Value!.Path == projectRootPath);

        if (folder.Value!.ProjectRoot)
        {
            name = folder.Text;
            projectFolder = folder;

            return true;
        }

        foreach (var section in selectedItemPathSections.Skip(1))
        {
            projectRootPath += "/" + section;

            folder = _treeItemData.Single(x => x.Value!.Path == projectRootPath);

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

    private void CollectFilesInProject(TreeItemData<FolderTreeItem> projectFolder)
    {
        var files = GetFiles(projectFolder);
        _files.AddRange(files.Select(x => x.Path));
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

    private void GoPrevious() => _currentFilePath = _files[_files.IndexOf(_currentFilePath) - 1];

    private void GoNext() => _currentFilePath = _files[_files.IndexOf(_currentFilePath) + 1];
}