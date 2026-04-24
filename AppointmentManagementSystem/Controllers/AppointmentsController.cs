using AppointmentManagementSystem.Data;
using AppointmentManagementSystem.Models;
using AppointmentManagementSystem.Models.ArchiveState;
using AppointmentManagementSystem.Services;
using AppointmentManagementSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace AppointmentManagementSystem.Controllers;

[Authorize]
public class AppointmentsController : Controller
{
    private readonly IAppointmentRepository _appointmentRepository;
    private readonly AccountDbContext _accountContext;

    public AppointmentsController(IAppointmentRepository appointmentRepository, AccountDbContext accountContext)
    {
        _appointmentRepository = appointmentRepository;
        _accountContext = accountContext;
    }

    public async Task<IActionResult> Index(bool? showArchived, string? searchQuery)
    {
        _appointmentRepository.CheckForExpiredAppointments();

        if (showArchived.HasValue)
        {
            ArchiveStateSingleton.Instance.IsViewingArchivedAppointments = showArchived.Value;
        }

        string userEmail = User.Identity.Name;
        if (userEmail == null)
            return NotFound();

        IEnumerable<Appointment> appointments = ArchiveStateSingleton.Instance.IsViewingArchivedAppointments ?
    _appointmentRepository.AllArchivedAppointments :
    _appointmentRepository.AllUpcomingAppointments;

        if (!User.IsInRole("Admin"))
        {
            appointments = appointments.Where(a => a.UserEmail == userEmail);
        }

        if (!string.IsNullOrEmpty(searchQuery))
        {
            ViewBag.PreviousSearchQuery = searchQuery;

            appointments = _appointmentRepository.SearchAppointments(searchQuery, appointments);
        }

        appointments = appointments
    .OrderBy(a => a.AppointmentDate)
    .ThenBy(a => a.AppointmentTime)
    .ToList();

        return View(appointments);
    }

    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var appointment = _appointmentRepository.GetAppointmentById(id);

        IActionResult authorizationResult = AppointmentValidation.CheckAppointmentValidation(this, appointment);
        if (authorizationResult != null)
            return authorizationResult;

        var user = _accountContext.Users.FirstOrDefault(u => u.Email == appointment.UserEmail);

        var viewModel = new ViewModel
        {
            UserAppointments = appointment,
            UserDetails = user
        };

        return View(viewModel);
    }

    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("AppointmentId,AppointmentSubject,AppointmentDate,AppointmentTime,UserEmail")] UpcomingAppointment appointment)
    {
        ModelState.Clear();
        if (ModelState.IsValid)
        {
            if (_accountContext.Users.FirstOrDefault(u => u.Email == appointment.UserEmail) != null)
            {
                _appointmentRepository.AddAppointment(appointment);
                return RedirectToAction(nameof(Index));
            }
        }

        string errorMessage = "No registered email found. " +
              "Please check the email entered is correct. " +
              "If so, please ask the user to register an account.";
        ModelState.AddModelError(nameof(appointment.UserEmail), errorMessage);
        return View(appointment);
    }

    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) { return NotFound(); }

        var appointment = _appointmentRepository.GetAppointmentById(id);

        IActionResult authorizationResult = AppointmentValidation.CheckAppointmentValidation(this, appointment);
        if (authorizationResult != null)
            return authorizationResult;

        return View(appointment);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("AppointmentId,AppointmentSubject,AppointmentDate,AppointmentTime,UserEmail")] UpcomingAppointment appointment)
    {
        IActionResult authorizationResult = AppointmentValidation.CheckAppointmentValidation(this, appointment);
        if (authorizationResult != null)
            return authorizationResult;

        if (ModelState.IsValid)
        {
            try
            {
                _appointmentRepository.UpdateAppointment(appointment);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.AppointmentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(appointment);
    }

    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) { return NotFound(); }

        var appointment = _appointmentRepository.GetAppointmentById(id);

        var user = await _accountContext.Users.FirstOrDefaultAsync(m => m.Email == appointment.UserEmail);

        IActionResult authorizationResult = AppointmentValidation.CheckAppointmentValidation(this, appointment);
        if (authorizationResult != null)
            return authorizationResult;

        var viewModel = new ViewModel();
        viewModel.UserAppointments = appointment;
        viewModel.UserDetails = user;

        return View(viewModel);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var appointment = _appointmentRepository.GetAppointmentById(id);

        if (appointment != null && (appointment.UserEmail == User.Identity.Name || User.IsInRole("Admin")))
        {
            _appointmentRepository.DeleteAppointmentById(id);
        }

        return RedirectToAction(nameof(Index));
    }

    private bool AppointmentExists(int id)
    {
        _appointmentRepository.GetAppointmentById(id);

        if (_appointmentRepository.GetAppointmentById(id) == null)
            return false;

        return true;
    }
}
