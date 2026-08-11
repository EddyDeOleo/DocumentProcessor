
using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.Domain.Entities
{
    public class Document
    {
        public Guid Id { get; private set; }
        public string FileName { get; private set; } = string.Empty;
        public string StorageUrl { get; private set; } = string.Empty;
        public DocumentStatus Status { get; private set; }
        public DateTime UploadedAt { get; private set; }
        public DateTime? ProcessedAt { get; private set; }

        // Navigation Property
        public ExtractedData? ExtractedData { get; private set; }

        // Parameterless constructor required by EF Core
        private Document() { }

        public Document(string fileName, string storageUrl)
        {
            Id = Guid.NewGuid();
            FileName = fileName;
            StorageUrl = storageUrl;
            Status = DocumentStatus.Pending;
            UploadedAt = DateTime.UtcNow;
        }

        public void MarkAsProcessing()
        {
            Status = DocumentStatus.Processing;
        }

        public void MarkAsCompleted(ExtractedData extractedData)
        {
            Status = DocumentStatus.Completed;
            ProcessedAt = DateTime.UtcNow;
            ExtractedData = extractedData;
        }

        public void MarkAsFailed()
        {
            Status = DocumentStatus.Failed;
            ProcessedAt = DateTime.UtcNow;
        }
    } 
}
