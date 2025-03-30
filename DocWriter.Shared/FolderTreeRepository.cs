using DocWriter.Shared.Models;

namespace DocWriter.Shared;

public class FolderTreeRepository : IFolderTreeRepository
{
    public async Task<IReadOnlyCollection<FolderTreeItem>> GetFolderTreeItemsAsync(FolderTreeItem parent)
    {
        await Task.Delay(300);
        return
        [
            new(
                "F1",
                "",
                FolderTreeItemType.Folder,
                [
                    new("Images", "F1/Images", FolderTreeItemType.Folder),
                    new("F3", "F1/F3", FolderTreeItemType.Folder),
                    new("M1.md", "F1/M1.md", FolderTreeItemType.Markdown)
                ]),
            new("F2", "", FolderTreeItemType.Folder),
        ];
    }

    public Task<FolderTreeItem> CreateFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();

    public Task<FolderTreeItem> UpdateFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();

    public Task DeleteFolderTreeItemAsync(FolderTreeItem item) => throw new NotImplementedException();
}