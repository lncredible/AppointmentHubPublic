using AppointmentManagementSystem.Data;
using AppointmentManagementSystem.Models.ArchiveState;

namespace AppointmentManagementSystem.Models;

public class AppointmentRepository : IAppointmentRepository
{

    private readonly AppointmentDbContext _context;

    public AppointmentRepository(AppointmentDbContext context)
    {
        _context = context;
    }

    public IEnumerable<UpcomingAppointment> AllUpcomingAppointments
    {
        get
        {
            return _context.appointments
    .OrderBy(a => a.AppointmentDate)
    .ThenBy(a => a.AppointmentTime);
        }
    }

    public IEnumerable<ArchivedAppointment> AllArchivedAppointments
    {
        get
        {
            return _context.archivedAppointments
    .OrderBy(a => a.AppointmentDate)
    .ThenBy(a => a.AppointmentTime);
        }
    }

    public Appointment GetAppointmentById(int? id)
    {
        if (ArchiveStateSingleton.Instance.IsViewingArchivedAppointments)
        {
            return _context.archivedAppointments.Find(id);
        }
        else
        {
            return _context.appointments.Find(id);
        }
    }

    public void AddAppointment(UpcomingAppointment? appointment)
    {
        _context.appointments.Add(appointment);
        _context.SaveChanges();
    }

    public void UpdateAppointment(UpcomingAppointment? appointment)
    {
        _context.appointments.Update(appointment);
        _context.SaveChanges();
    }

    public void DeleteAppointmentById(int? id)
    {
        if (ArchiveStateSingleton.Instance.IsViewingArchivedAppointments)
        {
            var archivedAppointment = _context.archivedAppointments.Find(id);
            if (archivedAppointment != null)
            {
                _context.archivedAppointments.Remove(archivedAppointment);
                _context.SaveChanges();
            }
        }
        else
        {
            var upcomingAppointment = _context.appointments.Find(id);
            if (upcomingAppointment != null)
            {
                DeleteAndArchive(upcomingAppointment);
            }
        }
    }

    private void DeleteAndArchive(UpcomingAppointment appointment)
    {
        var archivedAppointment = new ArchivedAppointment
        {
            UserEmail = appointment.UserEmail,
            AppointmentSubject = appointment.AppointmentSubject,
            AppointmentDate = appointment.AppointmentDate,
            AppointmentTime = appointment.AppointmentTime
        };

        _context.archivedAppointments.Add(archivedAppointment);
        _context.SaveChanges();

        _context.appointments.Remove(appointment);
        _context.SaveChanges();
    }

    public void CheckForExpiredAppointments()
    {
        var currentDateTime = DateTime.Now;

        var expiredAppointments = _context.appointments
.Where(a => a.AppointmentDate.Date < currentDateTime.Date ||
(a.AppointmentDate.Date == currentDateTime.Date && a.AppointmentTime.TimeOfDay < currentDateTime.TimeOfDay))
.ToList();

        foreach (var appointment in expiredAppointments)
            DeleteAndArchive(appointment);
    }

    public IEnumerable<Appointment> SearchAppointments(string searchQuery, IEnumerable<Appointment> appointments)
    {
        if (DateTime.TryParse(searchQuery, out DateTime searchDate))
        {
            appointments = appointments.Where(a => a.AppointmentDate.Date == searchDate.Date);
        }
        else
        {
            appointments = appointments.Where(a => a.AppointmentSubject.ToLower().Contains(searchQuery.ToLower()));
        }

        return appointments;
    }

    public void DeleteAllAppointmentByEmail(string email)
    {
        IEnumerable<UpcomingAppointment> upcomingAppointments = _context.appointments.Where(a => a.UserEmail == email);
        IEnumerable<ArchivedAppointment> archivedAppointments = _context.archivedAppointments.Where(a => a.UserEmail == email);

        foreach (UpcomingAppointment appointment in upcomingAppointments)
        {
            if (appointment != null)
            {
                _context.appointments.Remove(appointment);
            }
        }

        foreach (ArchivedAppointment appointment in archivedAppointments)
        {
            if (appointment != null)
            {
                _context.archivedAppointments.Remove(appointment);
            }
        }
        _context.SaveChanges();
    }
}
