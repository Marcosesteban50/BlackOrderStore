using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Black_order.Data;

/// <summary>
/// Development-only convenience: ensures an admin account exists so the panel can
/// be tested without manual SQL. Gated by Dev:AdminEmail and never runs in production.
/// </summary>
public static class DevAdminSeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger logger)
    {
        var email = configuration["Dev:AdminEmail"];

        if (string.IsNullOrWhiteSpace(email))
        {
            return;
        }

        try
        {
            using var scope = services.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var password = configuration["Dev:AdminPassword"] ?? "Admin123!";
            var user = await userManager.FindByEmailAsync(email);

            if (user is null)
            {
                user = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };

                var created = await userManager.CreateAsync(user, password);

                if (!created.Succeeded)
                {
                    logger.LogWarning(
                        "Dev admin could not be created: {Errors}",
                        string.Join("; ", created.Errors.Select(error => error.Description)));
                    return;
                }

                logger.LogInformation("Dev admin created: {Email}", email);
            }

            var claims = await userManager.GetClaimsAsync(user);

            if (claims.All(claim => claim.Type != "Admin"))
            {
                await userManager.AddClaimAsync(user, new Claim("Admin", "true"));
                logger.LogInformation("Admin claim granted to {Email}", email);
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Dev admin seed skipped.");
        }
    }
}
