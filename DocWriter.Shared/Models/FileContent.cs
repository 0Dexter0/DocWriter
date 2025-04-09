namespace DocWriter.Shared.Models;

public record FileContent
{
    public string Path { get; set; }

    public string Content { get; set; }
}