﻿namespace SummerBorn.Infrastructure.Configuration;

public class PhaseOfEducationConfiguration : IEntityTypeConfiguration<PhaseOfEducation>
{
    public void Configure(EntityTypeBuilder<PhaseOfEducation> builder)
    {
        builder.ToTable("phase_of_education");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .IsRequired();

        builder.Property(p => p.Version)
            .IsRowVersion();

    }
}
