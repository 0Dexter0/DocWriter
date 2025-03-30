namespace DocWriter.Shared.Models;

public record FolderTreeItem(string Name, string Path, FolderTreeItemType Type, IReadOnlyCollection<FolderTreeItem> Children = null);