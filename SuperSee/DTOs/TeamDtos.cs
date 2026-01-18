using System;
using System.Collections.Generic;

namespace SuperSee.DTOs;

public class TeamDto
{
    public Guid TeamId { get; set; }
    public string TeamName { get; set; }
    public Guid SupervisorId { get; set; }
    public string SupervisorName { get; set; }
    public Guid? TeamLeaderId { get; set; }
    public string TeamLeaderName { get; set; }
    public string AssignmentStatus { get; set; }
    public ProjectDto Project { get; set; }
    public List<StudentDto> Members { get; set; } = new();
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
    public Guid TeamLeaderId { get; set; }
}