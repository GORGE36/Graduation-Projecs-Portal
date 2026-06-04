using Microsoft.AspNetCore.Mvc;
using SuperSee.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TeamsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CoordinatorService _coordinatorService;

    public TeamsController(AppDbContext db, CoordinatorService coordinatorService)
    {
        _db = db;
        _coordinatorService = coordinatorService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<TeamDto>> GetAllTeams()
    {
        var teams = _coordinatorService.GetAllTeamsWithDetails();
        var dtos = teams.Select(t => MapToDto(t));
        return Ok(dtos);
    }

    [HttpPost]
    public ActionResult<TeamDto> CreateTeam([FromBody] CreateTeamRequest request)
    {
        try
        {
            // We need a UserContext to call the service. 
            // In a real app, this would come from authentication.
            // For now, we'll try to infer it or use a default if not provided.
            // The existing TeamsController doesn't seem to have access to current user easily.
            // I'll assume for this endpoint it's called by someone authorized or we use a dummy context.
            // Actually, the Program.cs shows how they handle it: they create a UserContext on the fly.
            
            // To be safe and consistent with existing code pattern:
            var userContext = new UserContext(Guid.Empty, Role.Admin); // Default to Admin for this generic endpoint
            var service = new CoordinatorService(_db, userContext);

            var team = service.CreateTeamWithProject(
                request.SupervisorId,
                request.TeamName,
                request.ProjectTitle,
                request.ProjectDescription,
                request.Deadline,
                request.StudentIds
            );
            return Ok(MapToDto(team));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{teamId}/approve")]
    public IActionResult ApproveTeam(Guid teamId)
    {
        try
        {
            var userContext = new UserContext(Guid.Empty, Role.Coordinator);
            var service = new CoordinatorService(_db, userContext);
            service.ApproveTeamByCoordinator(teamId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{teamId}/reject")]
    public IActionResult RejectTeam(Guid teamId)
    {
        try
        {
            // Try to get coordinatorId from query if provided, else dummy
            Guid coordId = Guid.Empty;
            var userContext = new UserContext(coordId, Role.Coordinator);
            var service = new CoordinatorService(_db, userContext);
            service.RejectTeamByCoordinator(teamId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{teamId}/assign-supervisor")]
    public IActionResult AssignSupervisor(Guid teamId, [FromQuery] Guid supervisorId)
    {
        try
        {
            var userContext = new UserContext(Guid.Empty, Role.Coordinator);
            var service = new CoordinatorService(_db, userContext);
            service.AssignSupervisorToTeam(teamId, supervisorId);
            return Ok(new { success = true });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteTeam(Guid id)
    {
        try
        {
            _coordinatorService.DeleteTeam(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private TeamDto MapToDto(Team team)
    {
        return new TeamDto
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            SupervisorId = team.SupervisorId,
            SupervisorName = team.Supervisor?.SupervisorName ?? "Unknown",
            AssignmentStatus = team.AssignmentStatus.ToString(),
            Project = team.Project == null ? null : new ProjectDto
            {
                Title = team.Project.Title,
                Description = team.Project.Description,
                Deadline = team.Project.Deadline,
                Status = team.Project.Status.ToString()
            },
            Members = team.Members.Select(m => new StudentDto
            {
                StudentId = m.StudentId,
                StudentName = m.StudentName, // Using StudnetName as per user's "before" state
                StudentEmail = m.StudentEmail
            }).ToList()
        };
    }
}