using System.Text;
using DocumentProcessor.Application.Interfaces;
using UglyToad.PdfPig;

namespace DocumentProcessor.Infrastructure.Services;

public class PdfPigExtractorService : ITextExtractorService
{
    public Task<string> ExtractTextAsync(Stream fileStream, CancellationToken cancellationToken = default)
    {
        var textBuilder = new StringBuilder();

        using (var pdfDocument = PdfDocument.Open(fileStream))
        {
            foreach (var page in pdfDocument.GetPages())
            {
                cancellationToken.ThrowIfCancellationRequested();
                textBuilder.AppendLine(page.Text);
            }
        }

        return Task.FromResult(textBuilder.ToString().Trim());
    }
}