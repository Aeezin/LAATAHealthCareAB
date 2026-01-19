using HealthCareAB_v1.Exceptions;
using HealthCareAB_v1.Models.DTOs;
using HealthCareAB_v1.Models.Entities;
using HealthCareAB_v1.Services.Interfaces;
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

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
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
                IsActive = true,
            };

            return CreatedAtAction(nameof(GetSchedule), new { id = response.Id }, response);
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

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetSchedule(int id)
    {
        try
        {
            var schedule = await _caregiverScheduleService.GetByIdAsync(id);

            var response = new CaregiverScheduleResponse
            {
                Id = schedule.Id,
                CaregiverId = schedule.CaregiverId,
                DayOfWeek = schedule.DayOfWeek,
                StartTime = schedule.StartTime,
                EndTime = schedule.EndTime,
                IsActive = schedule.IsActive,
            };

            return Ok(response);
        }
        catch (CaregiverScheduleNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
    [HttpGet("caregiver/{caregiverId}")]
    public async Task<IActionResult> GetCaregiverSchedules(int caregiverId)
    {
        try
        {
            var schedules = await _caregiverScheduleService.GetByCaregiverIdAsync(caregiverId);

            var response = schedules
                .Select(s => new CaregiverScheduleResponse
                {
                    Id = s.Id,
                    CaregiverId = s.CaregiverId,
                    DayOfWeek = s.DayOfWeek,
                    StartTime = s.StartTime,
                    EndTime = s.EndTime,
                    IsActive = s.IsActive,
                })
                .ToList();

            return Ok(response);
        }
        catch (Exception)
        {
            return StatusCode(500, "An unknown error occurred.");
        }
    }

    // [Authorize(Roles = "Patient", "Caregiver", "Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateSchedule(
        int id,
        [FromBody] UpdateCaregiverScheduleRequest req
    )
    {
        try
        {
            var updated = await _caregiverScheduleService.UpdateAsync(id, req);

            var response = new CaregiverScheduleResponse
            {
                Id = updated.Id,
                CaregiverId = updated.CaregiverId,
                DayOfWeek = updated.DayOfWeek,
                StartTime = updated.StartTime,
                EndTime = updated.EndTime,
                IsActive = updated.IsActive,
            };

            return Ok(response);
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
