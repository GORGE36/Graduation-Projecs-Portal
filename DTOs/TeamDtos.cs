using System;
using System.Collections.Generic;

namespace SuperSee.DTOs;

public class TeamDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; }
    public Guid? SupervisorId { get; set; }
    public string SupervisorName { get; set; }
    public string AssignmentStatus { get; set; }
    public ProjectDto Project { get; set; }
    public List<StudentDto> Members { get; set; } = new();
    public bool IsLateSubmission { get; set; }
    public List<FileSubmissionDto> FileSubmissions { get; set; } = new();
}

public class StudentDto
{
    public Guid StudentId { get; set; }
    public string StudentName { get; set; }
    public string StudentEmail { get; set; }
}

public class ProjectDto
{
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime Deadline { get; set; }
    public string Status { get; set; }
}

public class CreateTeamRequest
{
    public Guid SupervisorId { get; set; }
    public string TeamName { get; set; }
    public string ProjectTitle { get; set; }
    public string ProjectDescription { get; set; }
    public DateTime Deadline { get; set; }
    public List<Guid> StudentIds { get; set; }
}

public class FileSubmissionDto
{
    public Guid FileSubmissionId { get; set; }
    public string FileName { get; set; }
    public DateTime SubmittedAt { get; set; }
    public bool IsLate { get; set; }
    public List<FileCommentDto> Comments { get; set; } = new();
}

public class FileCommentDto
{
    public Guid FileCommentId { get; set; }
    public string SupervisorName { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateFileCommentRequest
{
    public string Content { get; set; }
}
