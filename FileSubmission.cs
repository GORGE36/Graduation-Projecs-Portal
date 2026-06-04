using System;
using System.Collections.Generic;

namespace SuperSee;

public class FileSubmission
{
    public Guid FileSubmissionId { get; set; }

    public Guid TeamId { get; set; }
    public Team Team { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

    public bool IsLate { get; set; }

    public List<FileComment> Comments { get; set; } = new();
}
