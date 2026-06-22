namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
internal sealed class CsaApplicationReviewConfiguration : IEntityTypeConfiguration<CsaApplicationReview>
{
    public void Configure(EntityTypeBuilder<CsaApplicationReview> builder)
    {
        _ = builder.ToTable("csa_application_review");

        _ = builder.HasKey(x => x.Id);
        _ = builder.Property(x => x.Id).IsRequired().ValueGeneratedNever();
        _ = builder.Property(x => x.SchoolId).IsRequired();
        _ = builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
        _ = builder.Property(x => x.ApplicationSuccessful).IsRequired();
        _ = builder.Property(x => x.Comment).HasMaxLength(4000).IsRequired();
        _ = builder.Property(x => x.SubmittedAtUtc).IsRequired();
        _ = builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();

        _ = builder.HasOne<School>()
            .WithMany()
            .HasForeignKey(x => x.SchoolId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired();

        _ = builder.HasMany(x => x.Reports)
            .WithOne()
            .HasForeignKey(x => x.CsaApplicationReviewId)
            .OnDelete(DeleteBehavior.Cascade);

        _ = builder.Navigation(x => x.Reports)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
