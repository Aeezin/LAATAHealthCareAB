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
    [HttpPost("{id}")]
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
                Status = created.Status
            };

            // return CreatedAtAction(nameof(GetAppointment),
            //& new { id = response.Id }, response);

            return Ok(response);
        }
        catch (AppointmentNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (AppointmentValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }
}
