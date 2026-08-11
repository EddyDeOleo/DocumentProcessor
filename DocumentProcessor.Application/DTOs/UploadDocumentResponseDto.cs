
using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.Application.DTOs;

    public record UploadDocumentResponseDto(
       Guid DocumentId,
       string FileName,
       DocumentStatus Status,
       DateTime UploadedAt
   );

