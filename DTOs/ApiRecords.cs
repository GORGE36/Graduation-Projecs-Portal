namespace SuperSee.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(bool Success, string Role, string Name, string RedirectUrl, Guid? UserId = null);
public record SwapMembersRequest(Guid Team1Id, Guid Student1Id, Guid Team2Id, Guid Student2Id);
// Allow SupervisorId to be supplied in the body to avoid relying solely on a query param
public record SendMessageToTeamRequest(Guid TeamId, string Content, Guid? SenderId = null, string Target = "Team"); // Target: Team, Supervisor, Both
public record SendMessageToSupervisorRequest(Guid SupervisorId, string Content, Guid? SenderId = null);
public record SendMessageToAllSupervisorsRequest(string Content, Guid? SenderId = null);
public record CreateTaskRequest(Guid TeamId, string Title, string Description, int InitialProgress, List<string> Milestones, DateTime? DueDate = null);
public record UpdateProgressRequest(int Progress);
public record UpdateMilestoneDueDateRequest(DateTime DueDate);
public record CreateTeamRequestByStudent(string TeamName, string ProjectTitle, string ProjectDescription, DateTime? Deadline, List<Guid> StudentIds, List<Guid> SuggestedSupervisorIds);
public record DecideRequestRequest(bool Accept, Guid? SupervisorId);
public record InviteToTeamRequest(List<string> InvitedNames);
public record FinalizeTeamRequestRequest(Guid TeamRequestId, string TeamName, string ProjectTitle, string ProjectDescription, DateTime? Deadline);
public record AddFileCommentRequest(Guid FileSubmissionId, string Content);

public record SetAvailabilityRequest(DateTime Date, string TimeSlot);
public record AssignCommitteeRequest(Guid TeamId, DateTime PresentationDate, Guid MainSupervisorId, Guid Sup2Id, Guid Sup3Id);
public record SubmitGradeRequest(Guid ProjectId, double Grade);
public record BroadcastDirectiveRequest(string Target, string Content);
