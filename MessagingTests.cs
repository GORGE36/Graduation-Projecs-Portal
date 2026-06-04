using Microsoft.EntityFrameworkCore;
using SuperSee;
using System;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class MessagingTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Message_PropertiesSetCorrectly()
    {
        // Arrange
        var senderId = Guid.NewGuid();
        var teamId = Guid.NewGuid();
        var content = "Hello Team!";
        
        // Act
        var message = new Message
        {
            SenderId = senderId,
            SenderRole = Role.Student,
            TeamId = teamId,
            Content = content,
            SentAt = DateTime.UtcNow
        };

        // Assert
        Assert.Equal(senderId, message.SenderId);
        Assert.Equal(Role.Student, message.SenderRole);
        Assert.Equal(teamId, message.TeamId);
        Assert.Equal(content, message.Content);
    }

    [Fact]
    public void SaveMessage_ToDatabase_Works()
    {
        // Arrange
        var db = GetDbContext();
        var team = new Team("Test Team", Guid.NewGuid());
        db.Teams.Add(team);
        db.SaveChanges();

        var message = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = Guid.NewGuid(),
            SenderRole = Role.Student,
            TeamId = team.TeamId,
            Content = "Test Content"
        };

        // Act
        db.Messages.Add(message);
        db.SaveChanges();

        // Assert
        var messageInDb = db.Messages.Find(message.MessageId);
        Assert.NotNull(messageInDb);
        Assert.Equal("Test Content", messageInDb.Content);
        Assert.Equal(team.TeamId, messageInDb.TeamId);
    }
}
