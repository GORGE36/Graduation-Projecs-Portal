using Microsoft.EntityFrameworkCore;
using SuperSee;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class ExtraTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    // --- Task & Milestone Tests ---

    [Fact]
    public void Task_Properties_Correct()
    {
        var taskId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var task = new SuperSee.Task
        {
            TaskId = taskId,
            Title = "Task 1",
            Description = "Desc 1",
            Progress = 50,
            TeamId = teamId,
            IsCompleted = false
        };

        Assert.Equal(taskId, task.TaskId);
        Assert.Equal("Task 1", task.Title);
        Assert.Equal(50, task.Progress);
        Assert.Equal(teamId, task.TeamId);
    }

    [Fact]
    public void Task_Initialization_DefaultValues()
    {
        var task = new SuperSee.Task();
        Assert.NotNull(task.Milestones);
        Assert.Equal(0, task.Progress);
        Assert.False(task.IsCompleted);
        Assert.True((DateTime.UtcNow - task.CreatedAt).TotalSeconds < 5);
    }

    [Fact]
    public void Milestone_Properties_Correct()
    {
        var mid = Guid.NewGuid();
        var tid = Guid.NewGuid();
        var m = new Milestone
        {
            MilestoneId = mid,
            Title = "M",
            TaskId = tid,
            IsCompleted = true,
            Order = 1
        };
        Assert.Equal(mid, m.MilestoneId);
        Assert.Equal(tid, m.TaskId);
        Assert.True(m.IsCompleted);
        Assert.Equal(1, m.Order);
    }

    [Fact]
    public void Team_Constructor_SetsDefaults()
    {
        var team = new Team("New Team");
        Assert.Equal("New Team", team.TeamName);
        Assert.Equal(AssignmentStatus.Pending, team.AssignmentStatus);
        Assert.NotNull(team.Members);
        Assert.NotNull(team.FileSubmissions);
    }

    [Fact]
    public void Project_Constructor_SetsProperties()
    {
        var tid = Guid.NewGuid();
        var deadline = DateTime.Now.AddDays(10);
        var project = new Project("Title", "Desc", deadline, tid);
        
        Assert.Equal("Title", project.Title);
        Assert.Equal("Desc", project.Description);
        Assert.Equal(deadline, project.Deadline);
        Assert.Equal(tid, project.TeamId);
        Assert.Equal(Status.NotStarted, project.Status);
    }

    [Fact]
    public void Project_Status_Change()
    {
        var project = new Project { Status = Status.InProgress };
        project.Status = Status.Done;
        Assert.Equal(Status.Done, project.Status);
    }

    [Fact]
    public void TeamInvitation_Status_Enum_Values()
    {
        Assert.Equal(0, (int)InvitationStatus.Pending);
        Assert.Equal(1, (int)InvitationStatus.Accepted);
        Assert.Equal(2, (int)InvitationStatus.Rejected);
    }

    [Fact]
    public void Student_Properties_Correct()
    {
        var sid = Guid.NewGuid();
        var student = new Student("Name", "email@t.com", "hash") { StudentId = sid };
        Assert.Equal("Name", student.StudentName);
        Assert.Equal("email@t.com", student.StudentEmail);
        Assert.Equal(sid, student.StudentId);
    }

    [Fact]
    public void Supervisor_Properties_Correct()
    {
        var cid = Guid.NewGuid();
        var supervisor = new Supervisor("Dr. Ahmed", "a@t.com", "h", cid);
        Assert.Equal("Dr. Ahmed", supervisor.SupervisorName);
        Assert.Equal(cid, supervisor.CoordinatorId);
        Assert.NotNull(supervisor.Teams);
    }

    [Fact]
    public void StudentDto_Mapping()
    {
        var dto = new SuperSee.DTOs.StudentDto
        {
            StudentId = Guid.NewGuid(),
            StudentName = "S",
            StudentEmail = "e"
        };
        Assert.Equal("S", dto.StudentName);
    }

    [Fact]
    public void TeamDto_Mapping()
    {
        var dto = new SuperSee.DTOs.TeamDto
        {
            TeamName = "T",
            SupervisorName = "Sup"
        };
        Assert.Equal("T", dto.TeamName);
    }

    [Fact]
    public void CreateTeamRequest_Validation()
    {
        var req = new SuperSee.DTOs.CreateTeamRequest
        {
            TeamName = "T",
            StudentIds = new List<Guid> { Guid.NewGuid() }
        };
        Assert.Single(req.StudentIds);
    }

    [Fact]
    public void AssignmentStatus_Enum_Values()
    {
        Assert.Equal(0, (int)AssignmentStatus.Pending);
        Assert.Equal(1, (int)AssignmentStatus.Accepted);
        Assert.Equal(2, (int)AssignmentStatus.Refused);
    }

    [Fact]
    public void Role_Enum_Values()
    {
        Assert.Equal(0, (int)Role.Student);
        Assert.Equal(1, (int)Role.Supervisor);
        Assert.Equal(2, (int)Role.Admin);
        Assert.Equal(3, (int)Role.Coordinator);
    }

    [Fact]
    public void UserContext_Role_Access()
    {
        var ctx = new UserContext(Guid.NewGuid(), Role.Supervisor);
        Assert.Equal(Role.Supervisor, ctx.Role);
    }

    [Fact]
    public void FileComment_Properties()
    {
        var fid = Guid.NewGuid();
        var sid = Guid.NewGuid();
        var comment = new FileComment
        {
            FileSubmissionId = fid,
            SupervisorId = sid,
            Content = "C"
        };
        Assert.Equal("C", comment.Content);
        Assert.Equal(fid, comment.FileSubmissionId);
    }

    [Fact]
    public void Student_Capability_Properties()
    {
        var sid = Guid.NewGuid();
        var cap = new StudentCapability { StudentId = sid, Name = "X" };
        Assert.Equal(sid, cap.StudentId);
        Assert.Equal("X", cap.Name);
    }

    [Fact]
    public void Task_Milestones_Relationship()
    {
        var db = GetDbContext();
        var team = new Team("Team");
        db.Teams.Add(team);
        db.SaveChanges();

        var task = new SuperSee.Task { TaskId = Guid.NewGuid(), Title = "Task", Description = "D", TeamId = team.TeamId };
        var milestone = new Milestone { MilestoneId = Guid.NewGuid(), Title = "M1", TaskId = task.TaskId };
        task.Milestones.Add(milestone);

        db.Tasks.Add(task);
        db.SaveChanges();

        var taskInDb = db.Tasks.Include(t => t.Milestones).FirstOrDefault(t => t.TaskId == task.TaskId);
        Assert.NotNull(taskInDb);
        Assert.Single(taskInDb.Milestones);
        Assert.Equal("M1", taskInDb.Milestones[0].Title);
    }

    [Fact]
    public void Milestone_Task_BackReference()
    {
        var task = new SuperSee.Task { Title = "T" };
        var milestone = new Milestone { Title = "M", Task = task };
        Assert.Equal("T", milestone.Task.Title);
    }

    [Fact]
    public void Team_Supervisor_Relationship()
    {
        var supervisor = new Supervisor("S", "e", "h", Guid.NewGuid());
        var team = new Team("T", supervisor.SupervisorId) { Supervisor = supervisor };
        Assert.Equal("S", team.Supervisor.SupervisorName);
    }

    [Fact]
    public void Team_Coordinator_Relationship()
    {
        var coord = new Coordinator("C", "e", "h");
        var team = new Team("T") { Coordinator = coord, CoordinatorId = coord.CoordinatorId };
        Assert.Equal("C", team.Coordinator.CoordinatorName);
    }


    [Fact]
    public void FileSubmission_Team_Relationship()
    {
        var team = new Team("T");
        var sub = new FileSubmission { Team = team, TeamId = team.TeamId };
        Assert.Equal("T", sub.Team.TeamName);
    }

    [Fact]
    public void FileComment_Supervisor_Relationship()
    {
        var sup = new Supervisor("S", "e", "h", Guid.NewGuid());
        var comment = new FileComment { Supervisor = sup, SupervisorId = sup.SupervisorId };
        Assert.Equal("S", comment.Supervisor.SupervisorName);
    }

    [Fact]
    public void Student_Team_Relationship()
    {
        var team = new Team("T");
        var student = new Student("S", "e", "h") { Team = team, TeamId = team.TeamId };
        Assert.Equal("T", student.Team.TeamName);
    }

    [Fact]
    public void StudentCapability_Student_Relationship()
    {
        var student = new Student("S", "e", "h");
        var cap = new StudentCapability { Student = student, StudentId = student.StudentId };
        Assert.Equal("S", cap.Student.StudentName);
    }

    [Fact]
    public void Project_Team_Relationship()
    {
        var team = new Team("T");
        var project = new Project { Team = team, TeamId = team.TeamId };
        Assert.Equal("T", project.Team.TeamName);
    }

    // --- TeamInvitation Tests ---

    [Fact]
    public void TeamInvitation_Status_Changes()
    {
        var invitation = new TeamInvitation
        {
            InvitationId = Guid.NewGuid(),
            Status = InvitationStatus.Pending
        };

        invitation.Status = InvitationStatus.Accepted;
        invitation.RespondedAt = DateTime.UtcNow;

        Assert.Equal(InvitationStatus.Accepted, invitation.Status);
        Assert.NotNull(invitation.RespondedAt);
    }

    [Fact]
    public void SaveInvitation_Works()
    {
        var db = GetDbContext();
        var student = new Student("S1", "s1@test.com", "h");
        db.Students.Add(student);
        db.SaveChanges();

        var inv = new TeamInvitation
        {
            InvitationId = Guid.NewGuid(),
            InvitedStudentId = student.StudentId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.TeamInvitations.Add(inv);
        db.SaveChanges();

        Assert.NotNull(db.TeamInvitations.Find(inv.InvitationId));
    }

    // --- Coordinator & HOD Tests ---

    [Fact]
    public void Coordinator_Initialization()
    {
        var coord = new Coordinator("Khaldoon", "k@test.com", "pass");
        Assert.Equal("Khaldoon", coord.CoordinatorName);
        Assert.Equal("k@test.com", coord.CoordinatorEmail);
    }

    [Fact]
    public void HOD_Initialization()
    {
        var hod = new HeadOfDepartment("Hamzah", "h@test.com", "pass");
        Assert.Equal("Hamzah", hod.Name);
    }

    [Fact]
    public void Coordinator_Relationship_With_Supervisors()
    {
        var db = GetDbContext();
        var coord = new Coordinator("C1", "c1@test.com", "p");
        db.Coordinators.Add(coord);
        db.SaveChanges();

        var supervisor = new Supervisor("S1", "s1@test.com", "p", coord.CoordinatorId);
        coord.Supervisors.Add(supervisor);
        db.Supervisors.Add(supervisor);
        db.SaveChanges();

        var coordInDb = db.Coordinators.Include(c => c.Supervisors).FirstOrDefault(c => c.CoordinatorId == coord.CoordinatorId);
        Assert.Single(coordInDb.Supervisors);
    }

    // --- Student Capability Tests ---

    [Fact]
    public void Student_Capability_Relationship()
    {
        var db = GetDbContext();
        var student = new Student("S1", "s1@test.com", "h");
        db.Students.Add(student);
        db.SaveChanges();

        var cap = new StudentCapability { StudentId = student.StudentId, Name = "C#" };
        db.Capabilities.Add(cap);
        db.SaveChanges();

        var studentInDb = db.Students.Include(s => s.Capabilities).FirstOrDefault(s => s.StudentId == student.StudentId);
        Assert.Single(studentInDb.Capabilities);
        Assert.Equal("C#", studentInDb.Capabilities[0].Name);
    }

    // --- More SupervisorService Edge Cases ---

    [Fact]
    public void AcceptAssignment_AlreadyRefused_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("S", "s@t.com", "h", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("T", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Accepted, SupervisorStatus = AssignmentStatus.Refused };
        db.Teams.Add(team);
        db.SaveChanges();

        var service = new SupervisorService(db, new UserContext(supervisor.SupervisorId, Role.Supervisor));
        Assert.Throws<InvalidOperationException>(() => service.AcceptAssignment(team.TeamId));
    }

    [Fact]
    public void RefuseAssignment_AlreadyAccepted_ThrowsException()
    {
        var db = GetDbContext();
        var supervisor = new Supervisor("S", "s@t.com", "h", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        var team = new Team("T", supervisor.SupervisorId) { AssignmentStatus = AssignmentStatus.Accepted, SupervisorStatus = AssignmentStatus.Accepted };
        db.Teams.Add(team);
        db.SaveChanges();

        var service = new SupervisorService(db, new UserContext(supervisor.SupervisorId, Role.Supervisor));
        Assert.Throws<InvalidOperationException>(() => service.RefuseAssignment(team.TeamId));
    }

    // --- More CoordinatorService Edge Cases ---

    [Fact]
    public void AddStudentToTeam_StudentAlreadyHasTeam_ThrowsException()
    {
        var db = GetDbContext();
        var team1 = new Team("T1");
        var team2 = new Team("T2");
        var student = new Student("S", "s@t.com", "h") { TeamId = team1.TeamId };
        db.Teams.AddRange(team1, team2);
        db.Students.Add(student);
        db.SaveChanges();

        var service = new CoordinatorService(db, new UserContext(Guid.NewGuid(), Role.Admin));
        Assert.Throws<InvalidOperationException>(() => service.AddStudentToTeam(team2.TeamId, student.StudentId));
    }

    [Fact]
    public void CreateTeamWithProject_DuplicateName_RefusedIncluded_ThrowsException()
    {
        var db = GetDbContext();
        db.Teams.Add(new Team("Dup") { AssignmentStatus = AssignmentStatus.Refused });
        db.SaveChanges();

        var service = new CoordinatorService(db, new UserContext(Guid.NewGuid(), Role.Admin));
        var sids = new List<Guid>();
        var student = new Student("S", "s@t.com", "h");
        db.Students.Add(student);
        db.SaveChanges();
        sids.Add(student.StudentId);

        Assert.Throws<InvalidOperationException>(() => service.CreateTeamWithProject(null, "Dup", "P", "D", DateTime.Now, sids));
    }

    [Fact]
    public void RemoveStudentFromTeam_StudentNotInTeam_ThrowsException()
    {
        var db = GetDbContext();
        var team = new Team("T");
        var student1 = new Student("S1", "s1@t.com", "h") { TeamId = team.TeamId };
        var student2 = new Student("S2", "s2@t.com", "h"); // Not in team
        team.Members.Add(student1);
        db.Teams.Add(team);
        db.Students.AddRange(student1, student2);
        db.SaveChanges();

        var service = new CoordinatorService(db, new UserContext(Guid.NewGuid(), Role.Admin));
        Assert.Throws<InvalidOperationException>(() => service.RemoveStudentFromTeam(team.TeamId, student2.StudentId));
    }

    [Fact]
    public void SwapMembers_DifferentSupervisor_Works()
    {
        var db = GetDbContext();
        var sup1 = new Supervisor("S1", "s1@t.com", "h", Guid.NewGuid());
        var sup2 = new Supervisor("S2", "s2@t.com", "h", Guid.NewGuid());
        var team1 = new Team("T1", sup1.SupervisorId);
        var team2 = new Team("T2", sup2.SupervisorId);
        var st1 = new Student("St1", "st1@t.com", "h") { TeamId = team1.TeamId, Team = team1 };
        var st2 = new Student("St2", "st2@t.com", "h") { TeamId = team2.TeamId, Team = team2 };
        team1.Members.Add(st1);
        team2.Members.Add(st2);
        db.Supervisors.AddRange(sup1, sup2);
        db.Teams.AddRange(team1, team2);
        db.Students.AddRange(st1, st2);
        db.SaveChanges();

        var service = new CoordinatorService(db, new UserContext(Guid.NewGuid(), Role.Admin));
        service.SwapMembersBetweenTeams(team1.TeamId, st1.StudentId, team2.TeamId, st2.StudentId);

        var ust1 = db.Students.Find(st1.StudentId);
        var ust2 = db.Students.Find(st2.StudentId);
        Assert.Equal(team2.TeamId, ust1.TeamId);
        Assert.Equal(team1.TeamId, ust2.TeamId);
    }

    [Fact]
    public void DeleteTeam_NullifiesStudentTeamId()
    {
        var db = GetDbContext();
        var team = new Team("T");
        var student = new Student("S", "s@t.com", "h") { TeamId = team.TeamId, Team = team };
        team.Members.Add(student);
        db.Teams.Add(team);
        db.Students.Add(student);
        db.SaveChanges();

        var service = new CoordinatorService(db, new UserContext(Guid.NewGuid(), Role.Admin));
        service.DeleteTeam(team.TeamId);

        var updatedStudent = db.Students.Find(student.StudentId);
        Assert.Null(updatedStudent.TeamId);
    }
}
