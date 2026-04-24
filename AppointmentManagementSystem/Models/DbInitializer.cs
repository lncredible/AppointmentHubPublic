using AppointmentManagementSystem.Areas.Identity.Data;
using AppointmentManagementSystem.Data;
using Microsoft.AspNetCore.Identity;

namespace AppointmentManagementSystem.Models;


public static class DbInitializer
{
    public static async void Seed(IApplicationBuilder applicationBuilder)
    {

        #region Seed Account data
        using (var scope = applicationBuilder.ApplicationServices.CreateScope())
        {
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        using (var scope = applicationBuilder.ApplicationServices.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
            DbUserInfo[] dbUsers = new DbUserInfo[]
{
                                new DbUserInfo
                {
                    FirstName = "Admin",
                    LastName = "User",
                    PhoneNumber = "1234567890",
                    Role = "Admin",
                    Email = "admin@admin.com",
                    Password = "h493yz96x5XyxYTfAOZdey/rL0Qe2fmESwmldH9Ph9g="
                },
                new DbUserInfo
                {
                    FirstName = "Test",
                    LastName = "User",
                    PhoneNumber = "9876543210",
                    Role = "User",
                    Email = "test@test.com",
                    Password = "9NgIcEyeC6DRUQwjgD2NEJ4lRV6N3rkMVpndW9u0VOE="
                },
                new DbUserInfo
                {
                    FirstName = "John",
                    LastName = "Doe",
                    PhoneNumber = "2012345678",
                    Role = "User",
                    Email = "email@email.com",
                    Password = "CYAW1j7zrwejgW47ldd36rgyOmUWHUJuwPRoOWvV5MM="
                }
};

            foreach (var newDbUser in dbUsers)
            {
                if (await userManager.FindByEmailAsync(newDbUser.Email) == null)
                {
                    var user = new AppUser
                    {
                        UserName = newDbUser.Email,
                        Email = newDbUser.Email,
                        EmailConfirmed = true,
                        FirstName = newDbUser.FirstName,
                        LastName = newDbUser.LastName,
                        PhoneNumber = newDbUser.PhoneNumber
                    };

                    var result = await userManager.CreateAsync(user, newDbUser.Password);

                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, newDbUser.Role);
                    }
                    else
                    {
                        throw new Exception($"Error creating seeded user data");
                    }
                }
            }
        }
        #endregion
    }
}

public class DbUserInfo
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string PhoneNumber { get; set; }
    public string Role { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}