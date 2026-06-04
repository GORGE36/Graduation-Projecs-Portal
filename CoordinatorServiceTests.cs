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
            students.Select(s => s.StudentId).ToList());

        Assert.NotNull(team);
        Assert.Equal("Team 1", team.TeamName);
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
            service.CreateTeamWithProject(supervisor.SupervisorId, "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
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
            service.CreateTeamWithProject(supervisor.SupervisorId, "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
    }

    [Fact]
    public void CreateTeamWithProject_UnauthorizedUser_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Supervisor);
        var service = new CoordinatorService(db, userContext);
        var students = CreateStudents(db, 3);

        Assert.Throws<UnauthorizedAccessException>(() => 
            service.CreateTeamWithProject(Guid.NewGuid(), "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
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
            service.CreateTeamWithProject(Guid.NewGuid(), "Team", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
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
            service.CreateTeamWithProject(supervisor.SupervisorId, "Duplicate Name", "Proj", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
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

    [Fact]
    public void CreateTeamWithProject_StudentRole_CreatesPendingTeam()
    {
        var db = GetDbContext();
        var studentId = Guid.NewGuid();
        var student = new Student("Requester", "req@test.com", "hash") { StudentId = studentId };
        db.Students.Add(student);
        db.SaveChanges();

        var userContext = new UserContext(studentId, Role.Student);
        var service = new CoordinatorService(db, userContext);

        var team = service.CreateTeamWithProject(null, "Student Team", "Title", "Desc", DateTime.Now.AddDays(1), new List<Guid> { studentId });

        Assert.Equal(AssignmentStatus.Pending, team.AssignmentStatus);
        Assert.Single(team.Members);
    }

    [Fact]
    public void ApproveTeamByCoordinator_Valid_UpdatesStatus()
    {
        var db = GetDbContext();
        var team = new Team("Pending Team") { AssignmentStatus = AssignmentStatus.Pending };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        service.ApproveTeamByCoordinator(team.TeamId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(AssignmentStatus.Accepted, updatedTeam.AssignmentStatus);
    }

    [Fact]
    public void RejectTeamByCoordinator_Valid_RemovesTeamReferences()
    {
        var db = GetDbContext();
        var team = new Team("Pending Team") { AssignmentStatus = AssignmentStatus.Pending };
        db.Teams.Add(team);
        db.SaveChanges(); // Save team first to get IDs if needed
        
        var student = new Student("S1", "s1@test.com", "hash") { Team = team, TeamId = team.TeamId };
        team.Members.Add(student);
        db.Students.Add(student);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        service.RejectTeamByCoordinator(team.TeamId);

        var updatedStudent = db.Students.Find(student.StudentId);
        Assert.Null(updatedStudent.TeamId);
        
        // CoordinatorService.RejectTeamByCoordinator calls db.Teams.Remove(team)
        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Null(updatedTeam);
    }

    [Fact]
    public void AssignSupervisorToTeam_Valid_UpdatesSupervisor()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        var supervisor = new Supervisor("Dr. X", "x@test.com", "hash", Guid.NewGuid());
        db.Teams.Add(team);
        db.Supervisors.Add(supervisor);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        service.AssignSupervisorToTeam(team.TeamId, supervisor.SupervisorId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(supervisor.SupervisorId, updatedTeam.SupervisorId);
    }

    [Fact]
    public void AddStudentToTeam_Valid_AddsStudent()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        var student = new Student("S1", "s1@test.com", "hash");
        db.Teams.Add(team);
        db.Students.Add(student);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        service.AddStudentToTeam(team.TeamId, student.StudentId);

        var updatedStudent = db.Students.Find(student.StudentId);
        Assert.Equal(team.TeamId, updatedStudent.TeamId);
    }

    [Fact]
    public void RemoveStudentFromTeam_Valid_RemovesStudent()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        var students = CreateStudents(db, 3);
        foreach(var s in students) { s.Team = team; s.TeamId = team.TeamId; team.Members.Add(s); }
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        service.RemoveStudentFromTeam(team.TeamId, students[0].StudentId);

        var updatedStudent = db.Students.Find(students[0].StudentId);
        Assert.Null(updatedStudent.TeamId);
    }

    [Fact]
    public void CreateTeamWithProject_MaxTeamsReached_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Busy Super", "busy@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        for (int i = 0; i < 5; i++)
        {
            db.Teams.Add(new Team($"Team {i}", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Accepted });
        }
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);
        var students = CreateStudents(db, 3);

        Assert.Throws<InvalidOperationException>(() => 
            service.CreateTeamWithProject(supervisor.SupervisorId, "Too Many", "Title", "Desc", DateTime.Now, students.Select(s => s.StudentId).ToList()));
    }

    [Fact]
    public void ApproveTeamByCoordinator_NotPending_ThrowsException()
    {
        var db = GetDbContext();
        var team = new Team("Accepted Team") { AssignmentStatus = AssignmentStatus.Accepted };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.ApproveTeamByCoordinator(team.TeamId));
    }

    [Fact]
    public void AssignSupervisorToTeam_NotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.AssignSupervisorToTeam(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public void AddStudentToTeam_StudentNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(team.TeamId, Guid.NewGuid()));
    }

    [Fact]
    public void SwapMembersBetweenTeams_StudentNotInTeam_ThrowsException()
    {
        var db = GetDbContext();
        var team1 = new Team("T1");
        var team2 = new Team("T2");
        var student1 = new Student("S1", "s1@test.com", "hash");
        db.Teams.AddRange(team1, team2);
        db.Students.Add(student1);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Coordinator);
        var service = new CoordinatorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.SwapMembersBetweenTeams(team1.TeamId, student1.StudentId, team2.TeamId, Guid.NewGuid()));
    }

    [Fact]
    public void GetAllTeamsWithDetails_ReturnsAllTeams()
    {
        var db = GetDbContext();
        db.Teams.Add(new Team("T1"));
        db.Teams.Add(new Team("T2"));
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new CoordinatorService(db, userContext);

        var teams = service.GetAllTeamsWithDetails();

        Assert.Equal(2, teams.Count);
    }
}
