using Microsoft.EntityFrameworkCore;
using SuperSee;
using System;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class FileSubmissionTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void FileSubmission_PropertiesSetCorrectly()
    {
        // Arrange
        var teamId = Guid.NewGuid();
        var fileName = "report.pdf";
        
        // Act
        var submission = new FileSubmission
        {
            TeamId = teamId,
            FileName = fileName,
            FilePath = "/uploads/report.pdf",
            SubmittedAt = DateTime.UtcNow,
            IsLate = false
        };

        // Assert
        Assert.Equal(teamId, submission.TeamId);
        Assert.Equal(fileName, submission.FileName);
        Assert.False(submission.IsLate);
    }

    [Fact]
    public void FileComment_Relationship_Works()
    {
        // Arrange
        var db = GetDbContext();
        var team = new Team("Team A", Guid.NewGuid());
        db.Teams.Add(team);
        db.SaveChanges();

        var submission = new FileSubmission
        {
            FileSubmissionId = Guid.NewGuid(),
            TeamId = team.TeamId,
            FileName = "doc.docx"
        };
        db.FileSubmissions.Add(submission);

        var supervisor = new Supervisor("Dr. X", "x@test.com", "h", Guid.NewGuid());
        db.Supervisors.Add(supervisor);
        db.SaveChanges();

        var comment = new FileComment
        {
            FileCommentId = Guid.NewGuid(),
            FileSubmissionId = submission.FileSubmissionId,
            SupervisorId = supervisor.SupervisorId,
            Content = "Good work"
        };

        // Act
        db.FileComments.Add(comment);
        db.SaveChanges();

        // Assert
        var subFromDb = db.FileSubmissions.Include(s => s.Comments).FirstOrDefault(s => s.FileSubmissionId == submission.FileSubmissionId);
        Assert.NotNull(subFromDb);
        Assert.Single(subFromDb.Comments);
        Assert.Equal("Good work", subFromDb.Comments[0].Content);
    }
}
