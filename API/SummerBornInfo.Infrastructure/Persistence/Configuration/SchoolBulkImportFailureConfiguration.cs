namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal sealed class SchoolBulkImportFailureConfiguration : IEntityTypeConfiguration<SchoolBulkImportFailure>
{
    public void Configure(EntityTypeBuilder<SchoolBulkImportFailure> builder)
    {
        builder.ToTable("school_bulk_import_failure");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .IsRequired();

        builder.Property(x => x.SchoolBulkImportRequestId)
            .IsRequired();

        builder.Property(x => x.LineNumber)
            .IsRequired();

        builder.Property(x => x.ErrorMessage)
            .IsRequired();
    }
}
