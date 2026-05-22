namespace SummerBornInfo.Infrastructure.Persistence.Configuration;

internal static class IdentityModelBuilderExtensions
{
    public static void ConfigureIdentityPersistence(this ModelBuilder builder)
    {
        _ = builder.Entity<ApplicationUser>().ToTable("application_user");
        _ = builder.Entity<ApplicationRole>().ToTable("application_role");
        _ = builder.Entity<IdentityUserClaim<Guid>>().ToTable("application_user_claim");
        _ = builder.Entity<IdentityUserLogin<Guid>>().ToTable("application_user_login");
        _ = builder.Entity<IdentityUserToken<Guid>>().ToTable("application_user_token");
        _ = builder.Entity<IdentityUserRole<Guid>>().ToTable("application_user_role");
        _ = builder.Entity<IdentityRoleClaim<Guid>>().ToTable("application_role_claim");
    }
}
