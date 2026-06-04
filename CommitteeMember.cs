using System;

namespace SuperSee;

public class CommitteeMember
{
    public Guid CommitteeMemberId { get; set; }
    public Guid ProjectId { get; set; }
    public Project Project { get; set; }
    public Guid SupervisorId { get; set; }
    public Supervisor Supervisor { get; set; }
    public bool IsMainSupervisor { get; set; } // The team's supervisor
    public double? Grade { get; set; } // Out of 50 if main, out of 25 if not
    public DateTime? GradedAt { get; set; }
}
