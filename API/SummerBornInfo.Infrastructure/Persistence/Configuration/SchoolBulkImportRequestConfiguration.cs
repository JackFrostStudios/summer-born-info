namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolBulkImportRequestConfiguration : IEntityTypeConfiguration<SchoolBulkImportRequest>
{
    public void Configure(EntityTypeBuilder<SchoolBulkImportRequest> builder)
    {
        builder.ToTable("school_bulk_import_request");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .IsRequired();

        builder.Property(s => s.Content)
            .IsRequired();
    }
}