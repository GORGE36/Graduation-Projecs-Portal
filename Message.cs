using System;

namespace SuperSee;

public class Message
{
    public Guid MessageId { get; set; }

    public Guid SenderId { get; set; }

    public Role SenderRole { get; set; }

    public Guid? TeamId { get; set; }
    public Team? Team { get; set; }

    public Guid? ReceiverSupervisorId { get; set; }
    public Supervisor? ReceiverSupervisor { get; set; }

    public string Content { get; set; } = string.Empty;
    public string? FileName { get; set; }
    public string? FilePath { get; set; }

    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}
