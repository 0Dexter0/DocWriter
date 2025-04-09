using DocWriter.Shared.Models;

namespace DocWriter.Shared;

public static class DataStorageMock
{
    public static FileContent[] ContentMock =
    [
        new()
        {
            Content = @"
file1 content

```mermaid
sequenceDiagram
    Alice->>Bob: Hello Bob, how are you ?
    Bob->>Alice: Fine, thank you. And you?
    create participant Carl
    Alice->>Carl: Hi Carl!
    create actor D as Donald
    Carl->>D: Hi!
    destroy Carl
    Alice-xCarl: We are too many
    destroy Bob
    Bob->>Alice: I agree
```",
            Path = "F1/M1.md"
        },
        new()
        {
            Content = "file2 content",
            Path = "F1/M2.md"
        }
    ];

    public static FolderTreeItem[] FolderTreeMock =
    [
        new("F1", "F1", FolderTreeItemType.Folder, true, true),
        new("Images", "F1/Images", FolderTreeItemType.Folder),
        new("M1.md", "F1/M1.md", FolderTreeItemType.Markdown),
        new("M2.md", "F1/M2.md", FolderTreeItemType.Markdown),
        new("F3", "F1/F3", FolderTreeItemType.Folder),
        new("F2", "F2", FolderTreeItemType.Folder)
    ];
}