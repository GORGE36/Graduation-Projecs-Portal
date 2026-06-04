using System;
using System.Collections.Generic;

namespace SuperSee;

public class Task
{
    public Guid TaskId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int Progress { get; set; } // 0 to 100
    public Guid TeamId { get; set; }
    public Team Team { get; set; }
    public List<Milestone> Milestones { get; set; } = new List<Milestone>();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DueDate { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class Milestone
{
    public Guid MilestoneId { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? DueDate { get; set; }
    public int Order { get; set; }
    public Guid TaskId { get; set; }
    public Task Task { get; set; }
}
