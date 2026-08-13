using DocumentProcessor.Domain.Entities;

namespace DocumentProcessor.Application.DTOs;

public static class DocumentMappingExtensions
{
    public static DocumentDetailsDto ToDto(this Document document) =>
        new(
            document.Id,
            document.FileName,
            document.StorageUrl,
            document.Status,
            document.UploadedAt,
            document.ProcessedAt,
            document.ExtractedData is not null ? document.ExtractedData.ToDto() : null
        );

    public static ExtractedDataDto ToDto(this ExtractedData data) =>
        new(
            data.DocumentType,
            data.VendorName,
            data.TaxId,
            data.TotalAmount,
            data.Currency,
            data.IssueDate,
            data.ConfidenceScore,
            data.RawJsonResponse
        );

    public static IEnumerable<DocumentDetailsDto> ToDtoList(this IEnumerable<Document> documents) =>
        documents.Select(d => d.ToDto());
}