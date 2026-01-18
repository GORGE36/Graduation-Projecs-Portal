using Microsoft.EntityFrameworkCore;
using Xunit;
using SuperSee;
using System;
using System.Linq;
using System.Collections.Generic;

namespace SuperSee.Tests;

public class SupervisorServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void GetTeamsForSupervisor_OwnTeams_ReturnsTeams()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        var teams = service.GetTeamsForSupervisor(supervisor.SupervisorId);

        Assert.Single(teams);
        Assert.Equal(team.TeamId, teams[0].TeamId);
    }

    [Fact]
    public void GetTeamsForSupervisor_OtherSupervisorTeams_ThrowsUnauthorized()
    {
        var db = GetDbContext();
        var supervisor1 = new Supervisor("Super 1", "super1@test.com", "hash", Guid.NewGuid());
        var supervisor2 = new Supervisor("Super 2", "super2@test.com", "hash", Guid.NewGuid());
        db.Supervisors.AddRange(supervisor1, supervisor2);
        db.SaveChanges();

        var userContext = new UserContext(supervisor1.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => 
            service.GetTeamsForSupervisor(supervisor2.SupervisorId));
    }

    [Fact]
    public void GetTeamsForSupervisor_AdminAccess_ReturnsTeams()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);
        
        var teams = service.GetTeamsForSupervisor(supervisor.SupervisorId);
        
        Assert.Single(teams);
    }

    [Fact]
    public void AcceptAssignment_ValidRequest_UpdatesStatus()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        service.AcceptAssignment(team.TeamId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(AssignmentStatus.Accepted, updatedTeam.AssignmentStatus);
    }

    [Fact]
    public void RefuseAssignment_ValidRequest_UpdatesStatus()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        service.RefuseAssignment(team.TeamId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(AssignmentStatus.Refused, updatedTeam.AssignmentStatus);
    }

    [Fact]
    public void AcceptAssignment_UnauthorizedUser_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        var otherSupervisor = new Supervisor("Other", "other@test.com", "hash", Guid.NewGuid());
        db.Supervisors.AddRange(supervisor, otherSupervisor);
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(otherSupervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void AcceptAssignment_NotPending_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Accepted };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void AcceptAssignment_StudentRole_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Student);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void GetTeamsForSupervisor_SupervisorNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);
        
        Assert.Throws<InvalidOperationException>(() => 
            service.GetTeamsForSupervisor(Guid.NewGuid()));
    }
}
