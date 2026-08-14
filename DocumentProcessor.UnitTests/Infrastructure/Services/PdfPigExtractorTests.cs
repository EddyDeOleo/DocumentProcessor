using DocumentProcessor.Infrastructure.Services;
using UglyToad.PdfPig.Content;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace DocumentProcessor.UnitTests.Infrastructure.Services;

public class PdfPigExtractorServiceTests
{
    private readonly PdfPigExtractorService _extractorService = new();

    [Fact]
    public async Task ExtractTextAsync_ValidPdfStream_ShouldReturnExtractedText()
    {
        // Arrange
        var expectedText = "Factura de Pruebas Altice Dominicana";
        using var pdfStream = CreateSamplePdfStream(expectedText);

        // Act
        var result = await _extractorService.ExtractTextAsync(pdfStream);

        // Assert
        Assert.NotNull(result);
        Assert.Contains(expectedText, result);
    }

    private static MemoryStream CreateSamplePdfStream(string textContent)
    {
        var builder = new PdfDocumentBuilder();
        var page = builder.AddPage(PageSize.A4);
        var font = builder.AddStandard14Font(Standard14Font.Helvetica);

        page.AddText(textContent, 12, new PdfPoint(25, 700), font);

        var bytes = builder.Build();
        return new MemoryStream(bytes);
    }
}