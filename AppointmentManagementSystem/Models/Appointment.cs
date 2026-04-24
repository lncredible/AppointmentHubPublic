using System.ComponentModel.DataAnnotations;

namespace AppointmentManagementSystem.Models;

public class Appointment
{
    [Key]
    [Display(Name = "Appointment Id")]
    public int AppointmentId { get; set; }

    [Required]
    [DataType(DataType.Text)]
    [Display(Name = "Appointment Subject/Reason")]
    public string AppointmentSubject { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Appointment Date")]
    public DateTime AppointmentDate { get; set; }

    [Required]
    [DataType(DataType.Time)]
    [Display(Name = "Appointment Time")]
    public DateTime AppointmentTime { get; set; }


    [Required]
    [DataType(DataType.EmailAddress)]
    [Display(Name = "Client Email")]
    public string UserEmail { get; set; }
}


public class UpcomingAppointment : Appointment
{

}

public class ArchivedAppointment : Appointment
{

}