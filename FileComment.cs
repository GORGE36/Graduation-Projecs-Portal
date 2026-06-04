using System;

namespace SuperSee;

public class FileComment
{
    public Guid FileCommentId { get; set; }

    public Guid FileSubmissionId { get; set; }
    public FileSubmission FileSubmission { get; set; }

    public Guid SupervisorId { get; set; }
    public Supervisor Supervisor { get; set; }

    public string Content { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
