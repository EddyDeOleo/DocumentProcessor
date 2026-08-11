using DocumentProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentProcessor.Infrastructure.Persistence.Configurations;

public class ExtractedDataConfiguration : IEntityTypeConfiguration<ExtractedData>
{
    public void Configure(EntityTypeBuilder<ExtractedData> builder)
    {
        builder.ToTable("ExtractedData");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.DocumentType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.VendorName)
            .HasMaxLength(200);

        builder.Property(e => e.TaxId)
            .HasMaxLength(100);

        builder.Property(e => e.TotalAmount)
            .HasPrecision(18, 2);

        builder.Property(e => e.Currency)
            .HasMaxLength(10);

        builder.Property(e => e.RawJsonResponse)
            .IsRequired()
            .HasColumnType("nvarchar(max)");
    }
}