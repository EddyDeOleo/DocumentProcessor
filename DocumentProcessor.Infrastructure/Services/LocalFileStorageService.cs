using DocumentProcessor.Application.Interfaces;

namespace DocumentProcessor.Infrastructure.Services;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _storageFolder;

    public LocalFileStorageService()
    {
        _storageFolder = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles");
        if (!Directory.Exists(_storageFolder))
        {
            Directory.CreateDirectory(_storageFolder);
        }
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
        var filePath = Path.Combine(_storageFolder, uniqueFileName);

        using (var destinationStream = new FileStream(filePath, FileMode.Create))
        {
            await fileStream.CopyToAsync(destinationStream, cancellationToken);
        }

        return filePath;
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        if (File.Exists(fileUrl))
        {
            File.Delete(fileUrl);
        }
        return Task.CompletedTask;
    }
}