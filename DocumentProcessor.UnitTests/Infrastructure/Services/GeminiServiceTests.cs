using System.Net;
using System.Text;
using DocumentProcessor.Domain.Enums;
using DocumentProcessor.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Moq;
using Moq.Protected;

namespace DocumentProcessor.UnitTests.Infrastructure.Services;

public class GeminiServiceTests
{
    [Fact]
    public async Task ProcessTextAsync_ValidGeminiResponse_ShouldParseAndReturnExtractedDataDto()
    {
        // Arrange
        var rawText = "Factura Claro Dominicana RNC 101001569 Total 1500 DOP";
        var geminiJsonResponse = """
        {
          "candidates": [
            {
              "content": {
                "parts": [
                  {
                    "text": "{\"documentType\":\"Invoice\",\"vendorName\":\"Claro Dominicana\",\"taxId\":\"101001569\",\"totalAmount\":1500.00,\"currency\":\"DOP\",\"issueDate\":\"2026-08-01\",\"confidenceScore\":0.96}"
                  }
                ]
              }
            }
          ]
        }
        """;

        var handlerMock = new Mock<HttpMessageHandler>();
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(geminiJsonResponse, Encoding.UTF8, "application/json")
            });

        var httpClient = new HttpClient(handlerMock.Object);

        var inMemorySettings = new Dictionary<string, string?>
        {
            {"Gemini:ApiKey", "test-api-key"},
            {"Gemini:Model", "gemini-3.5-flash"},
            {"Gemini:BaseUrl", "https://generativelanguage.googleapis.com/v1beta"}
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var service = new GeminiService(httpClient, configuration);

        // Act
        var result = await service.ProcessTextAsync(rawText);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(DocumentType.Invoice, result.DocumentType);
        Assert.Equal("Claro Dominicana", result.VendorName);
        Assert.Equal("101001569", result.TaxId);
        Assert.Equal(1500.00m, result.TotalAmount);
        Assert.Equal("DOP", result.Currency);
        Assert.Equal(0.96, result.ConfidenceScore);
    }

    [Fact]
    public void Constructor_MissingApiKey_ShouldThrowArgumentNullException()
    {
        // Arrange
        var httpClient = new HttpClient();
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>()) 
            .Build();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GeminiService(httpClient, configuration));
    }
}