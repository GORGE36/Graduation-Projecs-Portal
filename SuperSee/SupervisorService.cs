using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SuperSee;

public class SupervisorService
{
    private readonly AppDbContext db;
    private readonly UserContext userContext;

    public SupervisorService(AppDbContext db, UserContext userContext)
    {
        this.db = db;
        this.userContext = userContext;
    }

    public List<Team> GetTeamsForSupervisor(Guid supervisorId)
    {
        if (userContext.Role == Role.Supervisor && userContext.UserId != supervisorId)
            throw new UnauthorizedAccessException("You are not authorized to view teams for this supervisor.");
            
        if(supervisorId == Guid.Empty) throw new ArgumentNullException(nameof(supervisorId));
        if(!db.Supervisors.Any(s => s.SupervisorId == supervisorId)) throw new InvalidOperationException("Supervisor not found.");
        
        return db.Teams
            .Where(t => t.SupervisorId == supervisorId)
            .Include(t => t.Project)
            .Include(t => t.Members)
            .ThenInclude(m => m.Capabilities)
            .ToList();
    }

    public void AcceptAssignment(Guid teamId)
    {
        var team = db.Teams.Find(teamId)
            ?? throw new InvalidOperationException("Team not found.");

        if (userContext.Role != Role.Admin && (userContext.Role != Role.Supervisor || userContext.UserId != team.SupervisorId))
            throw new UnauthorizedAccessException("You are not authorized to accept this assignment.");

        if (team.AssignmentStatus != AssignmentStatus.Pending)
            throw new InvalidOperationException("Only pending assignments can be accepted.");

        team.AssignmentStatus = AssignmentStatus.Accepted;
        db.SaveChanges();
    }

    public void RefuseAssignment(Guid teamId)
    {
        var team = db.Teams.Find(teamId)
            ?? throw new InvalidOperationException("Team not found.");

        if (userContext.Role != Role.Admin && (userContext.Role != Role.Supervisor || userContext.UserId != team.SupervisorId))
            throw new UnauthorizedAccessException("You are not authorized to refuse this assignment.");

        if (team.AssignmentStatus != AssignmentStatus.Pending)
            throw new InvalidOperationException("Only pending assignments can be refused.");

        team.AssignmentStatus = AssignmentStatus.Refused;
        db.SaveChanges();
    }
}