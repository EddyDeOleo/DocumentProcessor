
using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.Application.DTOs;

    public record DocumentDetailsDto(
     Guid Id,
     string FileName,
     string StorageUrl,
     DocumentStatus Status,
     DateTime UploadedAt,
     DateTime? ProcessedAt,
     ExtractedDataDto? ExtractedData
    );

