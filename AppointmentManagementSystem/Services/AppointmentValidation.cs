using AppointmentManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentManagementSystem.Services;

public static class AppointmentValidation
{
    public static IActionResult CheckAppointmentValidation(this ControllerBase controller, Appointment appointment)
    {
        if (appointment == null)
        {
            return controller.NotFound();
        }

        if (!controller.User.IsInRole("Admin") && appointment.UserEmail != controller.User.Identity.Name)
        {
            return controller.Unauthorized();
        }

        return null;
    }

}
