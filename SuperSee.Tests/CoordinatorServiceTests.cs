using Microsoft.EntityFrameworkCore;
using Xunit;
using SuperSee;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee.Tests;

public class CoordinatorServiceTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private List<Student> CreateStudents(AppDbContext db, int count)
    {
        var students = new List<Student>();
        for (int i = 0; i < count; i++)
        {
            var s = new Student($"Student {i}", $"s{i}@test.com", "hash");
            db.Students.Add(s);
            students.Add(s);
        }
        db.SaveChanges();
        return students;
    }

    [Fact]
    public void CreateTeamWithProject_ValidRequest_CreatesTeamAndProject()
    {
        var db = GetDbContext();
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        db.Supervisors.Add(supervisor);
        var students = CreateStudents(db, 3);
        db.SaveChanges();

        var userContext = new UserContext(coordinator.CoordinatorId, Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        var team = service.CreateTeamWithProject(
            supervisor.SupervisorId,
            "Team 1",
            "Project 1",
            "Desc 1",
            DateTime.Now.AddDays(7),
            students.Select(s => s.StudentId).ToList(),
            students[0].StudentId);

        Assert.NotNull(team);
        Assert.Equal("Team 1", team.TeamName);
        Assert.Equal(students[0].StudentId, team.TeamLeaderId);
        Assert.NotNull(team.Project);
        Assert.Equal(3, team.Members.Count);
        
        var teamInDb = db.Teams.Include(t => t.Project).Include(t => t.Members).FirstOrDefault(t => t.TeamId == team.TeamId);
        Assert.NotNull(teamInDb);
        Assert.Equal(3, teamInDb.Members.Count);
    }

    [Fact]
    public void CreateTeamWithProject_TooFewMembers_ThrowsException()
    {
        var db = GetDbContext();
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        db.Supervisors.Add(supervisor);
        var students = new List<Student>(); // 0 students
        db.SaveChanges();

        var userContext = new UserContext(coordinator.CoordinatorId, Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.CreateTeamWithProject(supervisor.SupervisorId, "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList(), Guid.NewGuid()));
    }

    [Fact]
    public void CreateTeamWithProject_TooManyMembers_ThrowsException()
    {
        var db = GetDbContext();
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        db.Supervisors.Add(supervisor);
        var students = CreateStudents(db, 6);
        db.SaveChanges();

        var userContext = new UserContext(coordinator.CoordinatorId, Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.CreateTeamWithProject(supervisor.SupervisorId, "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList(), students[0].StudentId));
    }

    [Fact]
    public void CreateTeamWithProject_UnauthorizedUser_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Student);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => 
            service.CreateTeamWithProject(Guid.NewGuid(), "Team", "Proj", "Desc", DateTime.Now, new List<Guid>(), Guid.NewGuid()));
    }

    [Fact]
    public void DeleteTeam_ExistingTeam_RemovesTeam()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team to delete", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        service.DeleteTeam(team.TeamId);
        
        Assert.Null(db.Teams.Find(team.TeamId));
    }

    [Fact]
    public void SwapMembersBetweenTeams_ValidRequest_SwapsMembers()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);

        var team1 = new Team("Team 1", supervisor.SupervisorId);
        var student1 = new Student("Student 1", "s1@test.com", "hash");
        student1.Team = team1;
        team1.Members.Add(student1);

        var team2 = new Team("Team 2", supervisor.SupervisorId);
        var student2 = new Student("Student 2", "s2@test.com", "hash");
        student2.Team = team2;
        team2.Members.Add(student2);

        db.Teams.AddRange(team1, team2);
        db.Students.AddRange(student1, student2);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        service.SwapMembersBetweenTeams(team1.TeamId, student1.StudentId, team2.TeamId, student2.StudentId);

        var updatedStudent1 = db.Students.Find(student1.StudentId);
        var updatedStudent2 = db.Students.Find(student2.StudentId);

        Assert.Equal(team2.TeamId, updatedStudent1.TeamId);
        Assert.Equal(team1.TeamId, updatedStudent2.TeamId);
    }
    [Fact]
    public void CreateTeamWithProject_SupervisorNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);
        var students = CreateStudents(db, 3);

        Assert.Throws<InvalidOperationException>(() => 
            service.CreateTeamWithProject(Guid.NewGuid(), "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList(), students[0].StudentId));
    }

    [Fact]
    public void CreateTeamWithProject_DuplicateTeamName_ThrowsException()
    {
        var db = GetDbContext();
        var coordinator = new Coordinator("Coord", "coord@test.com", "hash");
        db.Coordinators.Add(coordinator);
        var supervisor = new Supervisor("Super", "super@test.com", "hash", coordinator.CoordinatorId);
        db.Supervisors.Add(supervisor);
        
        db.Teams.Add(new Team("Duplicate Name", supervisor.SupervisorId));
        db.SaveChanges();

        var userContext = new UserContext(coordinator.CoordinatorId, Role.Coordinator);
        var service = new CoordinatorService(db, userContext);
        var students = CreateStudents(db, 3);

        Assert.Throws<InvalidOperationException>(() => 
            service.CreateTeamWithProject(supervisor.SupervisorId, "Duplicate Name", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList(), students[0].StudentId));
    }

    [Fact]
    public void AddStudentToTeam_ExceedMaxSize_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team Full", supervisor.SupervisorId);
        var students = CreateStudents(db, 5);
        foreach (var s in students)
        {
            s.Team = team;
            s.TeamId = team.TeamId;
            team.Members.Add(s);
        }
        db.Teams.Add(team);
        db.SaveChanges();

        var newStudent = new Student("Extra", "extra@test.com", "hash");
        db.Students.Add(newStudent);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(team.TeamId, newStudent.StudentId));
    }

    [Fact]
    public void RemoveStudentFromTeam_BelowMinSize_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team Min", supervisor.SupervisorId);
        var students = CreateStudents(db, 1);
        foreach (var s in students)
        {
            s.Team = team;
            s.TeamId = team.TeamId;
            team.Members.Add(s);
        }
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.RemoveStudentFromTeam(team.TeamId, students[0].StudentId));
    }

    [Fact]
    public void DeleteTeam_NotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.DeleteTeam(Guid.NewGuid()));
    }

    [Fact]
    public void SwapMembersBetweenTeams_TeamNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.SwapMembersBetweenTeams(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
    }
}
