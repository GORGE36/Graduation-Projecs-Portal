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
        var team = new Team("Team 1", supervisor.SupervisorId) 
        { 
            AssignmentStatus = AssignmentStatus.Accepted,
            SupervisorStatus = AssignmentStatus.Pending 
        };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        service.AcceptAssignment(team.TeamId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(AssignmentStatus.Accepted, updatedTeam.SupervisorStatus);
    }

    [Fact]
    public void RefuseAssignment_ValidRequest_UpdatesStatus()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId) 
        { 
            AssignmentStatus = AssignmentStatus.Accepted,
            SupervisorStatus = AssignmentStatus.Pending 
        };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        service.RefuseAssignment(team.TeamId);

        var updatedTeam = db.Teams.Find(team.TeamId);
        Assert.Equal(AssignmentStatus.Refused, updatedTeam.SupervisorStatus);
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
    public void AcceptAssignment_NotAcceptedByCoordinator_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Pending };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void AcceptAssignment_AlreadyAcceptedBySupervisor_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team 1", supervisor.SupervisorId) 
        { 
            AssignmentStatus = AssignmentStatus.Accepted,
            SupervisorStatus = AssignmentStatus.Accepted 
        };
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
        var team = new Team("Team 1", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Pending };
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Student);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void GetTeamsForSupervisor_SupervisorNotFound_ReturnsEmptyList()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);
        
        var teams = service.GetTeamsForSupervisor(Guid.NewGuid());
        Assert.Empty(teams);
    }

    [Fact]
    public void AddCommentToFileSubmission_SubmissionNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.AddCommentToFileSubmission(Guid.NewGuid(), Guid.NewGuid(), "Content"));
    }

    [Fact]
    public void AddCommentToFileSubmission_Valid_AddsComment()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();
        var submission = new FileSubmission { TeamId = team.TeamId };
        db.FileSubmissions.Add(submission);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        service.AddCommentToFileSubmission(submission.FileSubmissionId, supervisor.SupervisorId, "Good job");

        var comment = db.FileComments.FirstOrDefault(c => c.FileSubmissionId == submission.FileSubmissionId);
        Assert.NotNull(comment);
        Assert.Equal("Good job", comment.Content);
    }

    [Fact]
    public void GetFileSubmission_Valid_ReturnsSubmission()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("Super", "super@test.com", "hash", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("Team", supervisor.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();
        var submission = new FileSubmission { TeamId = team.TeamId };
        db.FileSubmissions.Add(submission);
        db.SaveChanges();

        var userContext = new UserContext(supervisor.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        var result = service.GetFileSubmission(submission.FileSubmissionId);

        Assert.NotNull(result);
        Assert.Equal(submission.FileSubmissionId, result.FileSubmissionId);
    }

    [Fact]
    public void UploadFileSubmission_Valid_CreatesSubmission()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);

        service.UploadFileSubmission(team.TeamId, "test.pdf", "/path/test.pdf", DateTime.UtcNow.AddDays(1));

        var submission = db.FileSubmissions.FirstOrDefault(f => f.TeamId == team.TeamId);
        Assert.NotNull(submission);
        Assert.Equal("test.pdf", submission.FileName);
        Assert.False(submission.IsLate);
    }

    [Fact]
    public void UploadFileSubmission_Late_SetsIsLate()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        db.Teams.Add(team);
        db.SaveChanges();

        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);

        service.UploadFileSubmission(team.TeamId, "test.pdf", "/path/test.pdf", DateTime.UtcNow.AddDays(-1));

        var submission = db.FileSubmissions.FirstOrDefault(f => f.TeamId == team.TeamId);
        Assert.True(submission.IsLate);
    }

    [Fact]
    public void AddCommentToFileSubmission_Unauthorized_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor1 = new Supervisor("S1", "s1@test.com", "hash", Guid.NewGuid());
        var supervisor2 = new Supervisor("S2", "s2@test.com", "hash", Guid.NewGuid());
        db.Supervisors.AddRange(supervisor1, supervisor2);
        var team = new Team("Team", supervisor1.SupervisorId);
        db.Teams.Add(team);
        db.SaveChanges();
        var submission = new FileSubmission { TeamId = team.TeamId };
        db.FileSubmissions.Add(submission);
        db.SaveChanges();

        var userContext = new UserContext(supervisor2.SupervisorId, Role.Supervisor);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<UnauthorizedAccessException>(() => 
            service.AddCommentToFileSubmission(submission.FileSubmissionId, supervisor2.SupervisorId, "Content"));
    }

    [Fact]
    public void GetFileSubmission_NotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => service.GetFileSubmission(Guid.NewGuid()));
    }

    [Fact]
    public void UploadFileSubmission_TeamNotFound_ThrowsException()
    {
        var db = GetDbContext();
        var userContext = new UserContext(Guid.NewGuid(), Role.Admin);
        var service = new SupervisorService(db, userContext);

        Assert.Throws<InvalidOperationException>(() => 
            service.UploadFileSubmission(Guid.NewGuid(), "file", "path", DateTime.Now));
    }
}
