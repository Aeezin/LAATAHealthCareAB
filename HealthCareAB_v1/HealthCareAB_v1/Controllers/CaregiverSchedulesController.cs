using HealthCareAB_v1.Services.Interfaces;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HealthCareAB_v1.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CaregiverSchedulesController : ControllerBase
{
    private readonly ICaregiverScheduleService _caregiverScheduleService;

    public CaregiverSchedulesController(ICaregiverScheduleService caregiverScheduleService)
    {
        _caregiverScheduleService = caregiverScheduleService;
    }

    // [Authorize(Roles = "Admin", "Caregiver)]
    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateCaregiverScheduleRequest req)
    {
        try
        {
            var schedule = new CaregiverSchedule
            {
                CaregiverId = req.CaregiverId,
                DayOfWeek = req.DayOfWeek,
                StartTime = req.StartTime,
                EndTime = req.EndTime,
            };

            var created = await _caregiverScheduleService.CreateAsync(schedule);

            var response = new CaregiverScheduleResponse
            {
                Id = created.Id,
                CaregiverId = created.CaregiverId,
                DayOfWeek = created.DayOfWeek,
                StartTime = created.StartTime,
                EndTime = created.EndTime,
                IsActive = true
            };

            // return CreatedAtAction(nameof(GetSchedule),
            //      new { id = response.Id }, response);

            return Ok(response); // Replace this with the above commented-out CreatedAtAction once the GET endpoint has been constructed. 

        }
        catch (CaregiverScheduleNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (CaregiverScheduleValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }
}
