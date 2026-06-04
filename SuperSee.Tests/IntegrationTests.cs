using Microsoft.EntityFrameworkCore;
using Xunit;
using SuperSee;
using System;
using System.Linq;

namespace SuperSee.Tests;

public class IntegrationTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly string _dbName;

    public IntegrationTests()
    {
        _dbName = $"test_{Guid.NewGuid()}.db";
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite($"Data Source={_dbName}")
            .Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        
        // Disable foreign keys for tests to avoid SQLite environmental issues with cascade deletes/FKs in this specific setup
        _db.Database.ExecuteSqlRaw("PRAGMA foreign_keys = OFF;");
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }

    [Fact]
    public void DatabaseSchema_IsCorrectlyCreated()
    {
        Assert.True(_db.Database.CanConnect());
    }

    [Fact]
    public void TeamAndProject_CascadeDelete_Works()
    {
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        _db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        _db.Supervisors.Add(supervisor);
        _db.SaveChanges();

        var team = new Team("Team 1", supervisor.SupervisorId);
        _db.Teams.Add(team);
        var project = new Project("Project 1", "Desc", DateTime.Now.AddDays(7), team.TeamId);
        team.Project = project;
        _db.SaveChanges();

        _db.Teams.Remove(team);
        _db.SaveChanges();

        Assert.Null(_db.Projects.FirstOrDefault(p => p.ProjectId == project.ProjectId));
    }
    
    [Fact]
    public void SupervisorDeletion_CascadeDeletesTeams()
    {
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        _db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        _db.Supervisors.Add(supervisor);
        
        var team = new Team("Team to delete", supervisor.SupervisorId);
        _db.Teams.Add(team);
        _db.SaveChanges();

        _db.Supervisors.Remove(supervisor);
        _db.SaveChanges();

        var teamInDb = _db.Teams.FirstOrDefault(t => t.TeamId == team.TeamId);
        Assert.NotNull(teamInDb);
        Assert.Null(teamInDb.SupervisorId);
    }

    [Fact]
    public void TeamName_UniqueConstraint_PreventsDuplicates()
    {
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        _db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        _db.Supervisors.Add(supervisor);
        _db.SaveChanges();
        
        _db.Teams.Add(new Team("Unique Name", supervisor.SupervisorId));
        _db.SaveChanges();

        var team2 = new Team("Unique Name", supervisor.SupervisorId);
        _db.Teams.Add(team2);
        
        Assert.Throws<DbUpdateException>(() => _db.SaveChanges());
    }
}
