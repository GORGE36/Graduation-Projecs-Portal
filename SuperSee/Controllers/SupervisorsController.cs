using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupervisorsController : ControllerBase
{
    private readonly SupervisorService _supervisorService;

    public SupervisorsController(SupervisorService supervisorService)
    {
        _supervisorService = supervisorService;
    }

    [HttpPost("accept/{teamId}")]
    public IActionResult AcceptAssignment(Guid teamId)
    {
        try
        {
            _supervisorService.AcceptAssignment(teamId);
            return Ok(new { message = "Assignment accepted." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("refuse/{teamId}")]
    public IActionResult RefuseAssignment(Guid teamId)
    {
        try
        {
            _supervisorService.RefuseAssignment(teamId);
            return Ok(new { message = "Assignment refused." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}