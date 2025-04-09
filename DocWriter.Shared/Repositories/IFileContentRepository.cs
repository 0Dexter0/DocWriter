namespace DocWriter.Shared.Repositories;

public interface IFileContentRepository
{
    Task<string> GetFileContentAsync(string path);

    Task UpdateFileContentAsync(string path, string content);
}