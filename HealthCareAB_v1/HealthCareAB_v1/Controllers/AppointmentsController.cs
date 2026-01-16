using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.DTOs;
using Microsoft.AspNetCore.Mvc;
using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.Entities;

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
}
