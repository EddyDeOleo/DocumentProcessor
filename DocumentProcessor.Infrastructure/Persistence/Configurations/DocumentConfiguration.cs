using DocumentProcessor.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DocumentProcessor.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.ToTable("Documents");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(d => d.StorageUrl)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(d => d.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(d => d.UploadedAt)
            .IsRequired();

        // 1-to-1 Relationship with ExtractedData
        builder.HasOne(d => d.ExtractedData)
            .WithOne(e => e.Document)
            .HasForeignKey<ExtractedData>(e => e.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}