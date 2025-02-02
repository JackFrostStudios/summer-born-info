namespace SummerBornInfo.Web.Data;

public class SchoolContext(DbContextOptions<SchoolContext> options) : DbContext(options)
{
    public DbSet<EstablishmentGroup> EstablishmentGroup => Set<EstablishmentGroup>();
    public DbSet<EstablishmentStatus> EstablishmentStatus => Set<EstablishmentStatus>();
    public DbSet<EstablishmentType> EstablishmentType => Set<EstablishmentType>();
    public DbSet<LocalAuthority> LocalAuthority => Set<LocalAuthority>();
    public DbSet<PhaseOfEducation> PhaseOfEducation => Set<PhaseOfEducation>();
    public DbSet<School> School => Set<School>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder?.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
