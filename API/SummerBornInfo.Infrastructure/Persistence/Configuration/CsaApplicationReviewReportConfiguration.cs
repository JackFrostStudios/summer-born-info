namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
internal sealed class CsaApplicationReviewReportConfiguration : IEntityTypeConfiguration<CsaApplicationReviewReport>
{
    public void Configure(EntityTypeBuilder<CsaApplicationReviewReport> builder)
    {
        _ = builder.ToTable("csa_application_review_report");

        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Id).IsRequired().ValueGeneratedNever();
        _ = builder.Property(x => x.CsaApplicationReviewId).IsRequired();
        _ = builder.Property(x => x.Reason).HasMaxLength(50).IsRequired();
        _ = builder.Property(x => x.Details).HasMaxLength(1000);
        _ = builder.Property(x => x.ReporterFingerprint).HasMaxLength(128);
        _ = builder.Property(x => x.SubmittedAtUtc).IsRequired();
        _ = builder.Property(x => x.ResolvedAtUtc);

        _ = builder.HasIndex(x => new { x.CsaApplicationReviewId, x.ReporterFingerprint })
            .HasDatabaseName("ux_csa_application_review_report_open_fingerprint")
            .HasFilter("\"ResolvedAtUtc\" IS NULL AND \"ReporterFingerprint\" IS NOT NULL")
            .IsUnique();
    }
}
