using System;

namespace SuperSee;

public class Notification
{
    public Guid NotificationId { get; set; }
    public Guid UserId { get; set; } // Can be StudentId or SupervisorId
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty; // e.g., "NewMessage", "NewTask"
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsRead { get; set; } = false;
    public string? RelatedId { get; set; } // e.g., TaskId or MessageId
}
