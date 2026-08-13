using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using DocumentProcessor.Application.DTOs;
using DocumentProcessor.Application.Interfaces;
using DocumentProcessor.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace DocumentProcessor.Infrastructure.Services;

public class GeminiService : IAiService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _model;
    private readonly string _baseUrl;

    public GeminiService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["Gemini:ApiKey"]
            ?? throw new ArgumentNullException("Gemini:ApiKey", "Gemini API Key is missing from configuration.");

        _model = configuration["Gemini:Model"] ?? "gemini-3.5-flash";
        _baseUrl = configuration["Gemini:BaseUrl"] ?? "https://generativelanguage.googleapis.com/v1beta";
    }

    public async Task<ExtractedDataDto> ProcessTextAsync(string rawText, CancellationToken cancellationToken = default)
    {
        var endpoint = $"{_baseUrl}/models/{_model}:generateContent?key={_apiKey}";

        var prompt = $$"""
            Analyze the following extracted document text and parse the metadata into structured JSON format.

            DOCUMENT TEXT:
            {{rawText}}
            """;

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                response_mime_type = "application/json",
                response_schema = new
                {
                    type = "OBJECT",
                    properties = new
                    {
                        documentType = new { type = "STRING", @enum = new[] { "Invoice", "Receipt", "Contract", "TaxForm", "Unknown" } },
                        vendorName = new { type = "STRING" },
                        taxId = new { type = "STRING" },
                        totalAmount = new { type = "NUMBER" },
                        currency = new { type = "STRING" },
                        issueDate = new { type = "STRING" },
                        confidenceScore = new { type = "NUMBER" }
                    },
                    required = new[] { "documentType", "confidenceScore" }
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(endpoint, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseJson = await response.Content.ReadAsStringAsync(cancellationToken);
        using var doc = JsonDocument.Parse(responseJson);

        var rawExtractedJson = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString() ?? "{}";

        var parsed = JsonSerializer.Deserialize<GeminiParsedResponse>(rawExtractedJson)
            ?? throw new InvalidOperationException("Failed to deserialize Gemini response.");

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
            RawJsonResponse: rawExtractedJson
        );
    }
}

public record GeminiParsedResponse(
    [property: JsonPropertyName("documentType")] string DocumentType,
    [property: JsonPropertyName("vendorName")] string? VendorName,
    [property: JsonPropertyName("taxId")] string? TaxId,
    [property: JsonPropertyName("totalAmount")] decimal? TotalAmount,
    [property: JsonPropertyName("currency")] string? Currency,
    [property: JsonPropertyName("issueDate")] string? IssueDate,
    [property: JsonPropertyName("confidenceScore")] double ConfidenceScore
);