using Microsoft.EntityFrameworkCore;
using SuperSee;
using System;
using System.Linq;
using Xunit;

namespace SuperSee.Tests;

public class NotificationTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Notification_PropertiesSetCorrectly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var message = "You have a new task";
        
        // Act
        var notification = new Notification
        {
            UserId = userId,
            Message = message,
            Type = "NewTask",
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        };

        // Assert
        Assert.Equal(userId, notification.UserId);
        Assert.Equal(message, notification.Message);
        Assert.Equal("NewTask", notification.Type);
        Assert.False(notification.IsRead);
    }

    [Fact]
    public void SaveNotification_ToDatabase_Works()
    {
        // Arrange
        var db = GetDbContext();
        var notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Message = "Test Notification",
            Type = "Info"
        };

        // Act
        db.Notifications.Add(notification);
        db.SaveChanges();

        // Assert
        var inDb = db.Notifications.Find(notification.NotificationId);
        Assert.NotNull(inDb);
        Assert.Equal("Test Notification", inDb.Message);
    }
}
