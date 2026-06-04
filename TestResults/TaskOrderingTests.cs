using Microsoft.EntityFrameworkCore;
using Xunit;
using SuperSee;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Moq;
using SuperSee.DTOs;
using Microsoft.AspNetCore.Http;

namespace SuperSee.Tests;

public class TaskOrderingTests
{
    private AppDbContext GetDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public void Milestone_HasOrderField()
    {
        var milestone = new Milestone { Order = 5 };
        Assert.Equal(5, milestone.Order);
    }

    [Fact]
    public void Task_HasMilestones_Ordered()
    {
        var task = new SuperSee.Task();
        task.Milestones.Add(new Milestone { Title = "M2", Order = 1 });
        task.Milestones.Add(new Milestone { Title = "M1", Order = 0 });

        var ordered = task.Milestones.OrderBy(m => m.Order).ToList();
        Assert.Equal("M1", ordered[0].Title);
        Assert.Equal("M2", ordered[1].Title);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteTask_RemovesTaskAndMilestones()
    {
        var db = GetDbContext();
        var teamId = Guid.NewGuid();
        var task = new SuperSee.Task
        {
            TaskId = Guid.NewGuid(),
            TeamId = teamId,
            Title = "Task to delete",
            Description = "Test description",
            CreatedAt = DateTime.UtcNow
        };
        task.Milestones.Add(new Milestone { MilestoneId = Guid.NewGuid(), Title = "M1", Order = 0 });
        
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        // Verify exists
        Assert.NotNull(await db.Tasks.FindAsync(task.TaskId));
        Assert.NotEmpty(await db.Milestones.Where(m => m.TaskId == task.TaskId).ToListAsync());

        // Perform delete (matching Program.cs logic)
        var toDelete = await db.Tasks.FindAsync(task.TaskId);
        db.Tasks.Remove(toDelete);
        await db.SaveChangesAsync();

        // Verify gone
        Assert.Null(await db.Tasks.FindAsync(task.TaskId));
        Assert.Empty(await db.Milestones.Where(m => m.TaskId == task.TaskId).ToListAsync());
    }
}
