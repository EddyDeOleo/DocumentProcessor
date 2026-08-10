using DocumentProcessor.Domain.Enums;

namespace DocumentProcessor.Domain.Entities
{
    public class ExtractedData
    {
        public Guid Id { get; private set; }
        public Guid DocumentId { get; private set; }
        public DocumentType DocumentType { get; private set; }
        public string? VendorName { get; private set; }
        public string? TaxId { get; private set; }
        public decimal? TotalAmount { get; private set; }
        public string? Currency { get; private set; }
        public DateTime? IssueDate { get; private set; }
        public double ConfidenceScore { get; private set; }
        public string RawJsonResponse { get; private set; } = string.Empty;

        // Navigation Property
        public Document Document { get; private set; } = null!;

        private ExtractedData() { }

        public ExtractedData(
            Guid documentId,
            DocumentType documentType,
            string? vendorName,
            string? taxId,
            decimal? totalAmount,
            string? currency,
            DateTime? issueDate,
            double confidenceScore,
            string rawJsonResponse)
        {
            Id = Guid.NewGuid();
            DocumentId = documentId;
            DocumentType = documentType;
            VendorName = vendorName;
            TaxId = taxId;
            TotalAmount = totalAmount;
            Currency = currency;
            IssueDate = issueDate;
            ConfidenceScore = confidenceScore;
            RawJsonResponse = rawJsonResponse;
        }
    }
}
