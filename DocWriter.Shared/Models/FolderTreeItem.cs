namespace DocWriter.Shared.Models;

public record FolderTreeItem(
    string Name,
    string Path,
    FolderTreeItemType Type,
    bool Expandable = false,
    bool ProjectRoot = false,
    IReadOnlyCollection<FolderTreeItem> Children = null);