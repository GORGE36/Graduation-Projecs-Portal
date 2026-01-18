using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SuperSee;

public class CoordinatorService
{
    private readonly AppDbContext db;
    private readonly UserContext userContext;

    public CoordinatorService(AppDbContext db, UserContext userContext)
    {
        this.db = db;
        this.userContext = userContext;
    }
    
    public Team CreateTeamWithProject(
        Guid supervisorId,
        string teamName,
        string projectTitle,
        string projectDescription,
        DateTime deadline,
        List<Guid> studentIds,
        Guid teamLeaderId)
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can create teams.");

        if (studentIds == null || studentIds.Count < 1 || studentIds.Count > 5)
            throw new InvalidOperationException($"A team must have between 1 and 5 members.");

        if (!studentIds.Contains(teamLeaderId))
            throw new InvalidOperationException("The team leader must be a member of the team.");
        
        var supervisor = db.Supervisors
            .FirstOrDefault(s => s.SupervisorId == supervisorId)
            ?? throw new InvalidOperationException("Supervisor not found.");

        if (db.Teams.Any(t => t.TeamName == teamName))
            throw new InvalidOperationException($"A team with the name '{teamName}' already exists.");

        var students = db.Students.Where(s => studentIds.Contains(s.StudentId)).ToList();
        if (students.Count != studentIds.Count)
            throw new InvalidOperationException("One or more students not found.");

        if (students.Any(s => s.TeamId != null))
            throw new InvalidOperationException("One or more students are already assigned to a team.");
        
        var team = new Team
        (
            teamName,
            supervisor.SupervisorId,
            supervisor.CoordinatorId,
            teamLeaderId
        );

        foreach (var student in students)
        {
            student.Team = team;
            student.TeamId = team.TeamId; 
            team.Members.Add(student);
        }

        var project = new Project
        (
            projectTitle,
            projectDescription,
            deadline,
            team.TeamId   
        );

        team.Project = project;

        db.Teams.Add(team);   
        db.SaveChanges();

        return team;
    }

    public void AddStudentToTeam(Guid teamId, Guid studentId)
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can add members.");

        var team = db.Teams.Include(t => t.Members).FirstOrDefault(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("Team not found.");

        if (team.Members.Count >= 5)
            throw new InvalidOperationException($"Team cannot have more than 5 members.");

        var student = db.Students.Find(studentId)
            ?? throw new InvalidOperationException("Student not found.");

        if (student.TeamId != null)
            throw new InvalidOperationException("Student is already in a team.");

        student.TeamId = teamId;
        db.SaveChanges();
    }

    public void RemoveStudentFromTeam(Guid teamId, Guid studentId)
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can remove members.");

        var team = db.Teams.Include(t => t.Members).FirstOrDefault(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("Team not found.");

        if (team.Members.Count <= 1)
            throw new InvalidOperationException($"Team cannot have fewer than 1 members.");

        var student = team.Members.FirstOrDefault(m => m.StudentId == studentId)
            ?? throw new InvalidOperationException("Student is not in this team.");

        student.TeamId = null;
        db.SaveChanges();
    }

    public void DeleteTeam(Guid teamId)
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can delete teams.");

        var team = db.Teams
            .Include(t => t.Project)
            .Include(t => t.Members)
            .FirstOrDefault(t => t.TeamId == teamId);

        if (team == null)
            throw new InvalidOperationException("Team not found.");

        db.Teams.Remove(team);
        db.SaveChanges();
    }

    public void SwapMembersBetweenTeams(Guid team1Id, Guid student1Id, Guid team2Id, Guid student2Id)
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can swap members.");

        var team1 = db.Teams
            .Include(t => t.Members)
            .FirstOrDefault(t => t.TeamId == team1Id);

        var team2 = db.Teams
            .Include(t => t.Members)
            .FirstOrDefault(t => t.TeamId == team2Id);

        if (team1 == null || team2 == null)
            throw new InvalidOperationException("One or both teams not found.");

        var student1 = team1.Members.FirstOrDefault(m => m.StudentId == student1Id);
        var student2 = team2.Members.FirstOrDefault(m => m.StudentId == student2Id);

        if (student1 == null)
            throw new InvalidOperationException("Student 1 is not a member of team 1.");
        if (student2 == null)
            throw new InvalidOperationException("Student 2 is not a member of team 2.");
        
        team1.Members.Remove(student1);
        team2.Members.Remove(student2);
        
        student1.Team = team2;
        student1.TeamId = team2.TeamId;

        student2.Team = team1;
        student2.TeamId = team1.TeamId;
        
        team2.Members.Add(student1);
        team1.Members.Add(student2);

        db.SaveChanges();
    }

    public List<Team> GetAllTeamsWithDetails()
    {
        if (userContext.Role != Role.Coordinator && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("Only Coordinators or Admins can view all teams.");

        return db.Teams
            .Include(t => t.Project)
            .Include(t => t.Members)
                .ThenInclude(m => m.Capabilities)
            .ToList();
    }
}
