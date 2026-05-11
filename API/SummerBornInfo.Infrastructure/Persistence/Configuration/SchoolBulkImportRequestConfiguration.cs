namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolBulkImportRequestConfiguration : IEntityTypeConfiguration<SchoolBulkImportRequest>
{
    public void Configure(EntityTypeBuilder<SchoolBulkImportRequest> builder)
    {
        builder.ToTable("school_bulk_import_request");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.ContentId)
            .HasColumnType("oid")
            .IsRequired();

        builder.Property(s => s.LinesProcessed)
            .IsRequired();

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        builder.HasMany(s => s.Failures)
            .WithOne()
            .HasForeignKey(x => x.SchoolBulkImportRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Failures)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
