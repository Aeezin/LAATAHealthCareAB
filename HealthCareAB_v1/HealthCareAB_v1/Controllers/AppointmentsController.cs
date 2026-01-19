using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.Entities;

using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using HealthCareAB_v1.Models.DTOs.Appointment;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;

    public AppointmentsController(IAppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
    }

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
    [HttpPost]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentRequest req)
    {
        try
        {
            var appointment = new Appointment
            {
                PatientId = req.PatientId,
                CaregiverId = req.CaregiverId,
                Date = req.Date,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
                PatientNotes = req.PatientNotes
            };

            var created = await _appointmentService.CreateAsync(appointment);

            var response = new AppointmentResponse
            {
                Id = created.Id,
                PatientId = created.PatientId,
                CaregiverId = created.CaregiverId,
                Date = created.Date,
                StartTime = created.StartTime,
                EndTime = created.EndTime,
                PatientNotes = created.PatientNotes,
                CaregiverNotes = created.CaregiverNotes,
                Status = created.Status
            };

            // Uncomment when GET endpoint has been added:
            // return CreatedAtAction(nameof(GetAppointment),
            // new { id = response.Id }, response);
            return Ok(response);
        }
        catch (PatientNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (CaregiverNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (AppointmentNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (AppointmentValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (AppointmentLimitException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (AppointmentConflictException ex)
        {
            return Conflict(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetMyAppointments()
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

        try
        {

            var appointments = await _appointmentService.GetByUserIdAsync(userId);

            var responses = appointments.Select(a => new AppointmentResponse
            {
                Id = a.Id,
                PatientId = a.PatientId,
                CaregiverId = a.CaregiverId,
                Date = a.Date,
                StartTime = a.StartTime,
                EndTime = a.EndTime,
                Status = a.Status,
                PatientNotes = a.PatientNotes,
                CaregiverNotes = a.CaregiverNotes,

                CaregiverName = $"{a.Caregiver?.FirstName} {a.Caregiver?.LastName}",
                CaregiverSpecialisation = a.Caregiver?.Specialisation,
                Room = a.Caregiver?.Room,
                PatientName = $"{a.Patient?.FirstName} {a.Patient?.LastName}"
            }).ToList();

            return Ok(responses);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }

    [HttpGet("available-slots")]
    public async Task<IActionResult> GetAvailableTimeSlots(
        [FromQuery] GetAvailableSlotsQuery query)
    {
        if (query.StartDate >= query.EndDate)
            return BadRequest("Start date must be before end date");

        if ((query.EndDate - query.StartDate).TotalDays > 90)
            return BadRequest("Date range cannot exceed 90 days");

        var availableSlots = await _appointmentService.GetAvailableTimeSlotsAsync(
            query.CaregiverId,
            query.StartDate,
            query.EndDate);

        return Ok(availableSlots);
    }

    /// <summary>
    /// Mark an appointment as completed.
    /// </summary>
    /// <param name="id">Appointment ID</param>
    /// <param name="dto">DTO containing caregiver notes</param>
    /// <returns>Updated appointment</returns>
    [HttpPut("complete/{id}")]
    [Authorize(Roles = "Caregiver")]
    public async Task<IActionResult> Complete(int id, [FromBody] CompleteAppointmentRequest dto)
    {
        try
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
            {
                return Unauthorized("Invalid user token.");
            }

            var appointment = await _appointmentService.CompleteAppointmentAsync(id, userId, dto);

            var response = new CompleteAppointmentResponse
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                CaregiverId = appointment.CaregiverId,
                Date = appointment.Date,
                StartTime = appointment.StartTime,
                EndTime = appointment.EndTime,
                PatientNotes = appointment.PatientNotes,
                CaregiverNotes = appointment.CaregiverNotes,
                Status = appointment.Status
            };

            return Ok(response);
        }
        catch (AppointmentNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = ex.Message });
        }
        catch (AppointmentValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An error occurred while completing the appointment." });
        }
    }
}
