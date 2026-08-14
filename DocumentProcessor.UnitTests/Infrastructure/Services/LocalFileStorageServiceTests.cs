using DocumentProcessor.Infrastructure.Services;

namespace DocumentProcessor.UnitTests.Infrastructure.Services;

public class LocalFileStorageServiceTests : IDisposable
{
    private readonly LocalFileStorageService _storageService;
    private readonly List<string> _tempFilesToDelete = new();

    public LocalFileStorageServiceTests()
    {
        _storageService = new LocalFileStorageService();
    }

    [Fact]
    public async Task UploadFileAsync_ValidStream_ShouldSaveFileToDiskAndReturnPath()
    {
        // Arrange
        var fileName = "test_upload.pdf";
        using var stream = new MemoryStream("Sample file content for storage test"u8.ToArray());

        // Act
        var filePath = await _storageService.UploadFileAsync(stream, fileName);
        _tempFilesToDelete.Add(filePath); 

        // Assert
        Assert.True(File.Exists(filePath));
        Assert.Contains(fileName, filePath);
    }

    [Fact]
    public async Task DeleteFileAsync_ExistingFile_ShouldRemoveFileFromDisk()
    {
        // Arrange
        var fileName = "file_to_delete.txt";
        using var stream = new MemoryStream("Content to delete"u8.ToArray());
        var filePath = await _storageService.UploadFileAsync(stream, fileName);

        Assert.True(File.Exists(filePath));

        // Act
        await _storageService.DeleteFileAsync(filePath);

        // Assert
        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task DeleteFileAsync_NonExistingFile_ShouldNotThrowException()
    {
        // Arrange
        var nonExistingPath = Path.Combine(Directory.GetCurrentDirectory(), "UploadedFiles", "ghost_file.txt");

        // Act & Assert
        var exception = await Record.ExceptionAsync(() => _storageService.DeleteFileAsync(nonExistingPath));
        Assert.Null(exception);
    }

    public void Dispose()
    {
        foreach (var path in _tempFilesToDelete)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }
}