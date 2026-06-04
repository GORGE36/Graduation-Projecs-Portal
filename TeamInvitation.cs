namespace SuperSee;

public enum InvitationStatus
{
    Pending = 0,
    Accepted = 1,
    Rejected = 2
}

public class TeamInvitation
{
    public Guid InvitationId { get; set; }
    public Guid TeamRequestId { get; set; }
    public Guid InvitedStudentId { get; set; }
    public Student? InvitedStudent { get; set; }
    public InvitationStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RespondedAt { get; set; }
}
