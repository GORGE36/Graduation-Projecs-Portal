using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SuperSee;
using SuperSee.Controllers;
using SuperSee.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class TeamsControllerTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void GetAllTeams_ReturnsTeamsAsDtos()
    {
        // Arrange
        var db = GetDbContext();
        var coord = new Coordinator("Coord", "c@test.com", "h");
        db.Coordinators.Add(coord);
        var supervisor = new Supervisor("Super", "s@test.com", "h", coord.CoordinatorId);
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId);
        team.Supervisor = supervisor;
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
        var coordinatorService = new CoordinatorService(db, userContext);
        var controller = new TeamsController(db, coordinatorService);

        // Act
        var result = controller.GetAllTeams();

        // Assert
        var actionResult = Assert.IsType<ActionResult<IEnumerable<TeamDto>>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dtos = Assert.IsAssignableFrom<IEnumerable<TeamDto>>(okResult.Value);

        Assert.Single(dtos);
        Assert.Equal("Team 1", dtos.First().TeamName);
        Assert.Equal("Super", dtos.First().SupervisorName);
    }

    [Fact]
    public void CreateTeam_ValidRequest_ReturnsCreatedTeam()
    {
        // Arrange
        var db = GetDbContext();
        var coord = new Coordinator("Coord", "c@test.com", "h");
        db.Coordinators.Add(coord);
        var supervisor = new Supervisor("Super", "s@test.com", "h", coord.CoordinatorId);
        db.Supervisors.Add(supervisor);
        var students = new List<Student>
        {
            new Student("S1", "s1@test.com", "h"),
            new Student("S2", "s2@test.com", "h"),
            new Student("S3", "s3@test.com", "h")
        };
        db.Students.AddRange(students);
        db.SaveChanges();

        var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
        var coordinatorService = new CoordinatorService(db, userContext);
        var controller = new TeamsController(db, coordinatorService);

        var request = new CreateTeamRequest
        {
            SupervisorId = supervisor.SupervisorId,
            TeamName = "New Team",
            ProjectTitle = "New Proj",
            ProjectDescription = "Desc",
            Deadline = DateTime.Now.AddDays(7),
            StudentIds = students.Select(s => s.StudentId).ToList()
        };

        // Act
        var result = controller.CreateTeam(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TeamDto>>(result);
        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
        var dto = Assert.IsType<TeamDto>(okResult.Value);

        Assert.Equal("New Team", dto.TeamName);
        Assert.Equal(3, dto.Members.Count);
    }

    [Fact]
    public void CreateTeam_InvalidRequest_ReturnsBadRequest()
    {
        // Arrange
        var db = GetDbContext();
        var coordinatorService = new CoordinatorService(db, new UserContext(Guid.Empty, Role.Admin));
        var controller = new TeamsController(db, coordinatorService);

        var request = new CreateTeamRequest
        {
            // Missing supervisor, students etc will cause CoordinatorService to throw
            TeamName = "Invalid Team"
        };

        // Act
        var result = controller.CreateTeam(request);

        // Assert
        var actionResult = Assert.IsType<ActionResult<TeamDto>>(result);
        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }
}
