using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperSee.DTOs;
using SuperSee;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SupervisorsController : ControllerBase
{
    private readonly SupervisorService _supervisorService;
    private readonly AppDbContext _db;

    public SupervisorsController(SupervisorService supervisorService, AppDbContext db)
    {
        _supervisorService = supervisorService;
        _db = db;
    }

    // Feature 1 & 2: Get all teams for supervisor (sorted by late submissions first)
    [HttpGet("{supervisorId}/teams")]
    public ActionResult<IEnumerable<TeamDto>> GetSupervisorTeams(Guid supervisorId)
    {
        try
        {
            var teams = _supervisorService.GetTeamsForSupervisorSortedByLateSubmissions(supervisorId);
            var dtos = teams.Select(t => MapToTeamDto(t)).ToList();
            return Ok(dtos);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Feature 2: Access individual team separately
    [HttpGet("team/{teamId}")]
    public ActionResult<TeamDto> GetTeamDetail(Guid teamId)
    {
        try
        {
            var team = _db.Teams
                .Include(t => t.Project)
                .Include(t => t.Members)
                .Include(t => t.Supervisor)
                .Include(t => t.FileSubmissions)
                .ThenInclude(f => f.Comments)
                .ThenInclude(c => c.Supervisor)
                .FirstOrDefault(t => t.TeamId == teamId);

            if (team == null)
                return NotFound(new { message = "Team not found" });

            return Ok(MapToTeamDto(team));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Feature 3: Add comment to file submission
    [HttpPost("file-submission/{fileSubmissionId}/comment")]
    public IActionResult AddCommentToFile(Guid fileSubmissionId, [FromBody] CreateFileCommentRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Comment content cannot be empty" });

            // Get supervisor ID from context (in real app, from authentication)
            var supervisorId = Guid.Empty; // This would come from the authenticated user context

            _supervisorService.AddCommentToFileSubmission(fileSubmissionId, supervisorId, request.Content);
            return Ok(new { message = "Comment added successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Feature 3: Get file submission with comments
    [HttpGet("file-submission/{fileSubmissionId}")]
    public ActionResult<FileSubmissionDto> GetFileSubmission(Guid fileSubmissionId)
    {
        try
        {
            var submission = _supervisorService.GetFileSubmission(fileSubmissionId);
            return Ok(MapToFileSubmissionDto(submission));
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Feature 3: Upload file submission
    [HttpPost("team/{teamId}/upload-file")]
    public IActionResult UploadFile(Guid teamId, [FromQuery] string fileName, [FromQuery] string filePath)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(filePath))
                return BadRequest(new { message = "File name and path are required" });

            var team = _db.Teams.Include(t => t.Project).FirstOrDefault(t => t.TeamId == teamId);
            if (team == null)
                return NotFound(new { message = "Team not found" });

            var deadline = team.Project?.Deadline ?? DateTime.Now.AddMonths(6);
            _supervisorService.UploadFileSubmission(teamId, fileName, filePath, deadline);
            return Ok(new { message = "File uploaded successfully" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
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

    private TeamDto MapToTeamDto(Team team)
    {
        var isLate = team.FileSubmissions?.Any(f => f.IsLate) ?? false;
        var fileSubmissions = team.FileSubmissions?.Select(f => MapToFileSubmissionDto(f)).ToList() ?? new();

        return new TeamDto
        {
            TeamId = team.TeamId,
            TeamName = team.TeamName,
            SupervisorId = team.SupervisorId,
            SupervisorName = team.Supervisor?.SupervisorName ?? "Unknown",
            AssignmentStatus = team.AssignmentStatus.ToString(),
            IsLateSubmission = isLate,
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
                StudentName = m.StudentName,
                StudentEmail = m.StudentEmail
            }).ToList(),
            FileSubmissions = fileSubmissions
        };
    }

    private FileSubmissionDto MapToFileSubmissionDto(FileSubmission submission)
    {
        return new FileSubmissionDto
        {
            FileSubmissionId = submission.FileSubmissionId,
            FileName = submission.FileName,
            SubmittedAt = submission.SubmittedAt,
            IsLate = submission.IsLate,
            Comments = submission.Comments.Select(c => new FileCommentDto
            {
                FileCommentId = c.FileCommentId,
                SupervisorName = c.Supervisor?.SupervisorName ?? "Unknown",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).OrderBy(c => c.CreatedAt).ToList()
        };
    }
}
