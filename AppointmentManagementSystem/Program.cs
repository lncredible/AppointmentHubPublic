using AppointmentManagementSystem.Areas.Identity.Data;
using AppointmentManagementSystem.Data;
using AppointmentManagementSystem.Models;
using AppointmentManagementSystem.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var AppointmentDbConnection = builder.Configuration.GetConnectionString("AppointmentDbConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        var AccountDbConnection = builder.Configuration.GetConnectionString("AccountDbConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

        builder.Services.AddScoped<ISmsService, SmsService>();
        builder.Services.AddHostedService<SmsAutomationService>();

        builder.Services.AddDbContext<AppointmentDbContext>(options =>
    options.UseSqlServer(AppointmentDbConnection));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddDbContext<AccountDbContext>(options =>
    options.UseSqlServer(AccountDbConnection));
        builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        builder.Services.AddDefaultIdentity<AppUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>().AddEntityFrameworkStores<AccountDbContext>();
        builder.Services.AddControllersWithViews();

        builder.Services.AddRazorPages();

        builder.Services.AddAuthorization();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseMigrationsEndPoint();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseAuthorization();

        app.MapControllerRoute(
name: "default",
pattern: "{controller=Home}/{action=Index}/{id?}");

        app.MapRazorPages();

        DbInitializer.Seed(app);

        app.Run();
    }
}