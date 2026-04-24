using AppointmentManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace AppointmentManagementSystem.Data;

public class AppointmentDbContext : DbContext
{
    public AppointmentDbContext(DbContextOptions<AppointmentDbContext> options)
        : base(options)
    {

    }

    public DbSet<UpcomingAppointment> appointments { get; set; }
    public DbSet<ArchivedAppointment> archivedAppointments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UpcomingAppointment>().ToTable("Appointments");
        modelBuilder.Entity<ArchivedAppointment>().ToTable("ArchivedAppointments");
    }
}
