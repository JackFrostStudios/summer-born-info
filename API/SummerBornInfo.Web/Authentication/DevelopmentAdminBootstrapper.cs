namespace SummerBornInfo.Web.Authentication;

internal sealed class DevelopmentAdminBootstrapper(
    IOptions<DevelopmentAdminBootstrapOptions> options,
    RoleManager<ApplicationRole> roleManager,
    UserManager<ApplicationUser> userManager)
    : IDevelopmentAdminBootstrapper
{
    public async Task UpsertAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configuredAdmin = GetConfiguredAdmin();
        if (configuredAdmin is null)
        {
            return;
        }

        await EnsureAdminRoleExistsAsync();
        var adminUser = await GetOrCreateAdminUserAsync(configuredAdmin.Value.Email, configuredAdmin.Value.Password);
        await EnsureAdminPasswordAsync(adminUser, configuredAdmin.Value.Password);
        await EnsureAdminRoleAssignmentAsync(adminUser);
    }

    private ConfiguredAdmin? GetConfiguredAdmin()
    {
        var adminUserEmail = options.Value.AdminUserEmail?.Trim();
        var adminUserPassword = options.Value.AdminUserPassword;
        var hasAdminUserEmail = !string.IsNullOrWhiteSpace(adminUserEmail);
        var hasAdminUserPassword = !string.IsNullOrWhiteSpace(adminUserPassword);

        if (!hasAdminUserEmail && !hasAdminUserPassword)
        {
            return null;
        }

        if (!hasAdminUserEmail || !hasAdminUserPassword)
        {
            throw new InvalidOperationException(
                "Development admin bootstrap requires both AdminUserEmail and AdminUserPassword when either value is configured.");
        }

        return new ConfiguredAdmin(adminUserEmail!, adminUserPassword!);
    }

    private async Task<ApplicationUser> GetOrCreateAdminUserAsync(string adminUserEmail, string adminUserPassword)
    {
        var adminUser = await userManager.FindByEmailAsync(adminUserEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                Email = adminUserEmail,
                UserName = adminUserEmail,
                EmailConfirmed = true,
            };

            var createResult = await userManager.CreateAsync(adminUser, adminUserPassword);
            EnsureSucceeded(createResult, "Failed to create the development admin user.");
            return adminUser;
        }

        var userUpdated = false;

        if (!StringComparer.Ordinal.Equals(adminUser.Email, adminUserEmail))
        {
            adminUser.Email = adminUserEmail;
            userUpdated = true;
        }

        if (!StringComparer.Ordinal.Equals(adminUser.UserName, adminUserEmail))
        {
            adminUser.UserName = adminUserEmail;
            userUpdated = true;
        }

        if (!adminUser.EmailConfirmed)
        {
            adminUser.EmailConfirmed = true;
            userUpdated = true;
        }

        if (userUpdated)
        {
            var updateResult = await userManager.UpdateAsync(adminUser);
            EnsureSucceeded(updateResult, "Failed to update the development admin user.");
        }

        return adminUser;
    }

    private async Task EnsureAdminPasswordAsync(ApplicationUser adminUser, string adminUserPassword)
    {
        var hasPassword = await userManager.HasPasswordAsync(adminUser);
        var passwordMatches = hasPassword
            && await userManager.CheckPasswordAsync(adminUser, adminUserPassword);

        if (passwordMatches)
        {
            return;
        }

        if (hasPassword)
        {
            var removePasswordResult = await userManager.RemovePasswordAsync(adminUser);
            EnsureSucceeded(removePasswordResult, "Failed to remove the existing development admin password.");
        }

        var addPasswordResult = await userManager.AddPasswordAsync(adminUser, adminUserPassword);
        EnsureSucceeded(addPasswordResult, "Failed to set the configured development admin password.");
    }

    private async Task EnsureAdminRoleAssignmentAsync(ApplicationUser adminUser)
    {
        if (await userManager.IsInRoleAsync(adminUser, ApplicationRoleNames.Admin))
        {
            return;
        }

        var addToRoleResult = await userManager.AddToRoleAsync(adminUser, ApplicationRoleNames.Admin);
        EnsureSucceeded(addToRoleResult, "Failed to assign the Admin role to the development admin user.");
    }

    private async Task EnsureAdminRoleExistsAsync()
    {
        if (await roleManager.RoleExistsAsync(ApplicationRoleNames.Admin))
        {
            return;
        }

        var createRoleResult = await roleManager.CreateAsync(new ApplicationRole
        {
            Name = ApplicationRoleNames.Admin,
        });
        EnsureSucceeded(createRoleResult, "Failed to create the Admin role for development bootstrap.");
    }

    private static void EnsureSucceeded(IdentityResult result, string errorMessage)
    {
        if (result.Succeeded)
        {
            return;
        }

        throw new InvalidOperationException(
            $"{errorMessage} {string.Join("; ", result.Errors.Select(error => error.Description))}");
    }

    private readonly record struct ConfiguredAdmin(string Email, string Password);
}
