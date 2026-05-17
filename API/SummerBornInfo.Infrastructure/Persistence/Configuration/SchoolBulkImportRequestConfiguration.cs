namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolBulkImportRequestConfiguration : IEntityTypeConfiguration<SchoolBulkImportRequest>
{
    public void Configure(EntityTypeBuilder<SchoolBulkImportRequest> builder)
    {
        _ = builder.ToTable("school_bulk_import_request");

        _ = builder.HasKey(s => s.Id);
        _ = builder.Property(s => s.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(s => s.ContentId)
            .HasColumnType("oid")
            .IsRequired();

        _ = builder.Property(s => s.LinesProcessed)
            .IsRequired();

        _ = builder.Property(s => s.Status)
            .HasConversion<string>()
            .IsRequired();

        _ = builder.HasMany(s => s.Failures)
            .WithOne()
            .HasForeignKey(x => x.SchoolBulkImportRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = builder.Navigation(x => x.Failures)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
