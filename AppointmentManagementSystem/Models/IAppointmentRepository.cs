namespace AppointmentManagementSystem.Models;

public interface IAppointmentRepository
{
    IEnumerable<UpcomingAppointment> AllUpcomingAppointments { get; }

    IEnumerable<ArchivedAppointment> AllArchivedAppointments { get; }

    Appointment GetAppointmentById(int? id);

    void AddAppointment(UpcomingAppointment? appointment);

    void UpdateAppointment(UpcomingAppointment appointment);

    void DeleteAppointmentById(int? id);

    void DeleteAllAppointmentByEmail(string email);

    IEnumerable<Appointment> SearchAppointments(string searchQuery, IEnumerable<Appointment> appointments);

    void CheckForExpiredAppointments();
}