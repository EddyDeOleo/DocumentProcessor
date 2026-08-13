using Microsoft.AspNetCore.Http;

namespace DocumentProcessor.Application.Dtos;

public record UploadDocumentDto(IFormFile File);