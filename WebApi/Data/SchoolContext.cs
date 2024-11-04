namespace SummerBornInfo.Data;

public class SchoolContext(DbContextOptions<SchoolContext> options) : DbContext(options)
{
    public DbSet<EstablishmentGroup> EstablishmentGroup { get; set; }
    public DbSet<EstablishmentStatus> EstablishmentStatus { get; set; }
    public DbSet<EstablishmentType> EstablishmentType { get; set; }
    public DbSet<LocalAuthority> LocalAuthority { get; set; }
    public DbSet<PhaseOfEducation> PhaseOfEducation { get; set; }
    public DbSet<School> School { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
