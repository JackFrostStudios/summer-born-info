namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
internal sealed class SchoolBulkImportFailureConfiguration : IEntityTypeConfiguration<SchoolBulkImportFailure>
{
    public void Configure(EntityTypeBuilder<SchoolBulkImportFailure> builder)
    {
        _ = builder.ToTable("school_bulk_import_failure");

        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Id)
            .IsRequired()
            .ValueGeneratedNever();

        _ = builder.Property(x => x.SchoolBulkImportRequestId)
            .IsRequired();

        _ = builder.Property(x => x.LineNumber)
            .IsRequired();

        _ = builder.Property(x => x.ErrorMessage)
            .IsRequired();
    }
}
