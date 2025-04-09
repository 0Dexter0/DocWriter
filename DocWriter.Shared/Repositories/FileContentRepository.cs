namespace DocWriter.Shared.Repositories;

internal class FileContentRepository : IFileContentRepository
{
    private readonly Dictionary<string, string> _cache = [];

    public async Task<string> GetFileContentAsync(string path)
    {
        // if (_cache.TryGetValue(path, out var content))
        // {
        //     return content;
        // }

        string content = DataStorageMock.ContentMock.FirstOrDefault(x => x.Path == path)?.Content ?? string.Empty;
        _cache[path] = content;

        await Task.Delay(1100);

        return content;
    }

    public Task UpdateFileContentAsync(string path, string content)
    {
        var data = DataStorageMock.ContentMock.FirstOrDefault(x => x.Path == path);

        if (data is not null)
        {
            data.Content = content;
            _cache[path] = content;
        }

        return Task.CompletedTask;
    }
}