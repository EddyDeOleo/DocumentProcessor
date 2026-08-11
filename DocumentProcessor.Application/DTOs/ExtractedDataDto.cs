

using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.Application.DTOs;

    public record ExtractedDataDto(
      DocumentType DocumentType,
      string? VendorName,
      string? TaxId,
      decimal? TotalAmount,
      string? Currency,
      DateTime? IssueDate,
      double ConfidenceScore,
      string RawJsonResponse
  );

