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
    private readonly CoordinatorService _coordinatorService;

    public TeamsController(CoordinatorService coordinatorService)
    {
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
            var team = _coordinatorService.CreateTeamWithProject(
                request.SupervisorId,
                request.TeamName,
                request.ProjectTitle,
                request.ProjectDescription,
                request.Deadline,
                request.StudentIds,
                request.TeamLeaderId
            );
            return Ok(MapToDto(team));
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
            TeamLeaderId = team.TeamLeaderId,
            TeamLeaderName = team.TeamLeader?.StudentName ?? "None", // Using StudnetName as per user's "before" state
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