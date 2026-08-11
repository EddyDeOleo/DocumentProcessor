using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Domain.Enums;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;

namespace DocumentProcessor.Infrastructure.Services;

public class OpenAiService : IOpenAiService
{
    private readonly ChatClient _chatClient;

    public OpenAiService(IConfiguration configuration)
    {
        var apiKey = configuration["OpenAI:ApiKey"]
            ?? throw new ArgumentNullException("OpenAI:ApiKey", "OpenAI API key is missing from configuration.");

        _chatClient = new ChatClient("gpt-4o-mini", apiKey);
    }

    public async Task<ExtractedDataDto> ProcessTextAsync(string rawText, CancellationToken cancellationToken = default)
    {
        var prompt = $$"""
        Analyze the following extracted document text and parse the metadata into structured JSON format.

        DOCUMENT TEXT:
        {{rawText}}
        """;

        ChatCompletionOptions options = new()
        {
            ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
                jsonSchemaFormatName: "document_extraction",
                jsonSchema: BinaryData.FromString("""
            {
              "type": "object",
              "properties": {
                "documentType": { "type": "string", "enum": ["Invoice", "Receipt", "Contract", "TaxForm", "Unknown"] },
                "vendorName": { "type": ["string", "null"] },
                "taxId": { "type": ["string", "null"] },
                "totalAmount": { "type": ["number", "null"] },
                "currency": { "type": ["string", "null"] },
                "issueDate": { "type": ["string", "null"] },
                "confidenceScore": { "type": "number" }
              },
              "required": ["documentType", "confidenceScore"],
              "additionalProperties": false
            }
            """)
            )
        };

        ChatCompletion completion = await _chatClient.CompleteChatAsync(
            [new UserChatMessage(prompt)],
            options,
            cancellationToken);

        var jsonResponse = completion.Content[0].Text;

        var parsed = JsonSerializer.Deserialize<OpenAiParsedResponse>(jsonResponse)
            ?? throw new InvalidOperationException("Failed to deserialize OpenAI response.");

        Enum.TryParse<DocumentType>(parsed.DocumentType, true, out var documentType);
        DateTime.TryParse(parsed.IssueDate, out var parsedDate);

        return new ExtractedDataDto(
            DocumentType: documentType,
            VendorName: parsed.VendorName,
            TaxId: parsed.TaxId,
            TotalAmount: parsed.TotalAmount,
            Currency: parsed.Currency,
            IssueDate: parsedDate != default ? parsedDate : null,
            ConfidenceScore: parsed.ConfidenceScore,
            RawJsonResponse: jsonResponse
        );
    }

    private record OpenAiParsedResponse(
        [property: JsonPropertyName("documentType")] string DocumentType,
        [property: JsonPropertyName("vendorName")] string? VendorName,
        [property: JsonPropertyName("taxId")] string? TaxId,
        [property: JsonPropertyName("totalAmount")] decimal? TotalAmount,
        [property: JsonPropertyName("currency")] string? Currency,
        [property: JsonPropertyName("issueDate")] string? IssueDate,
        [property: JsonPropertyName("confidenceScore")] double ConfidenceScore
    );
}