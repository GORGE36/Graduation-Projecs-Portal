using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace SuperSee;

public class SupervisorService
{
    private readonly AppDbContext db;
    private readonly UserContext userContext;

    public SupervisorService(AppDbContext db, UserContext userContext)
    {
        this.db = db;
        this.userContext = userContext;
    }

    public List<Team> GetTeamsForSupervisor(Guid supervisorId)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        if (userContext.Role == Role.Supervisor && userContext.UserId != supervisorId)
            throw new UnauthorizedAccessException("لا يمكن الوصول لفرق مشرف آخر.");

        return db.Teams
            .Where(t => t.SupervisorId == supervisorId)
            .Include(t => t.Members)
            .Include(t => t.Project)
            .ToList();
    }

    public List<Team> GetTeamsForSupervisorSortedByLateSubmissions(Guid supervisorId)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        if (userContext.Role == Role.Supervisor && userContext.UserId != supervisorId)
            throw new UnauthorizedAccessException("لا يمكن الوصول لفرق مشرف آخر.");

        return db.Teams
            .Where(t => t.SupervisorId == supervisorId)
            .Include(t => t.Members)
            .Include(t => t.Project)
            .Include(t => t.FileSubmissions)
            .OrderByDescending(t => t.FileSubmissions.Any(f => f.IsLate))
            .ThenByDescending(t => t.FileSubmissions.Any() ? t.FileSubmissions.Max(f => f.SubmittedAt) : DateTime.MinValue)
            .ToList();
    }

    public void AcceptAssignment(Guid teamId)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("عذراً، لم يتم العثور على هذا الفريق في النظام.");

        if (team.SupervisorId != userContext.UserId && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("عذراً، هذا الفريق ليس ضمن قائمة الفرق المسندة إليك.");

        if (team.AssignmentStatus != AssignmentStatus.Accepted)
            throw new InvalidOperationException("عذراً، لا يمكن قبول المهمة حالياً لأن موافقة المنسق ما زالت قيد الانتظار.");

        if (team.SupervisorStatus != AssignmentStatus.Pending)
            throw new InvalidOperationException("عذراً، لقد قمت مسبقاً باتخاذ إجراء (قبول أو رفض) لهذا الفريق.");

        team.SupervisorStatus = AssignmentStatus.Accepted;
        db.SaveChanges();
    }

    public void RefuseAssignment(Guid teamId)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("عذراً، لم يتم العثور على هذا الفريق في النظام.");

        if (team.SupervisorId != userContext.UserId && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("عذراً، هذا الفريق ليس ضمن قائمة الفرق المسندة إليك.");

        if (team.AssignmentStatus != AssignmentStatus.Accepted)
            throw new InvalidOperationException("عذراً، لا يمكن رفض المهمة حالياً لأن موافقة المنسق ما زالت قيد الانتظار.");

        if (team.SupervisorStatus != AssignmentStatus.Pending)
            throw new InvalidOperationException("عذراً، لقد قمت مسبقاً باتخاذ إجراء (قبول أو رفض) لهذا الفريق.");

        team.SupervisorStatus = AssignmentStatus.Refused;
        db.SaveChanges();
    }

    public void AddCommentToFileSubmission(Guid fileSubmissionId, Guid supervisorId, string content)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        var submission = db.FileSubmissions
            .Include(f => f.Team)
            .FirstOrDefault(f => f.FileSubmissionId == fileSubmissionId)
            ?? throw new InvalidOperationException("لم يتم العثور على ملف التسليم.");

        if (submission.Team.SupervisorId != supervisorId && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("هذا الملف لا ينتمي لفريقك.");

        var comment = new FileComment
        {
            FileCommentId = Guid.NewGuid(),
            FileSubmissionId = fileSubmissionId,
            SupervisorId = supervisorId,
            Content = content,
            CreatedAt = DateTime.UtcNow
        };

        db.FileComments.Add(comment);
        db.SaveChanges();
    }

    public FileSubmission GetFileSubmission(Guid fileSubmissionId)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        var submission = db.FileSubmissions
            .Include(f => f.Team)
            .Include(f => f.Comments)
            .ThenInclude(c => c.Supervisor)
            .FirstOrDefault(f => f.FileSubmissionId == fileSubmissionId)
            ?? throw new InvalidOperationException("لم يتم العثور على ملف التسليم.");

        if (submission.Team.SupervisorId != userContext.UserId && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("هذا الملف لا ينتمي لفريقك.");

        return submission;
    }

    public void UploadFileSubmission(Guid teamId, string fileName, string filePath, DateTime deadline)
    {
        if (userContext.Role != Role.Supervisor && userContext.Role != Role.Admin)
            throw new UnauthorizedAccessException("صلاحية غير مسموح بها.");

        var team = db.Teams.FirstOrDefault(t => t.TeamId == teamId)
            ?? throw new InvalidOperationException("الفريق غير موجود.");

        var submission = new FileSubmission
        {
            FileSubmissionId = Guid.NewGuid(),
            TeamId = teamId,
            FileName = fileName,
            FilePath = filePath,
            SubmittedAt = DateTime.UtcNow,
            IsLate = DateTime.UtcNow > deadline
        };

        db.FileSubmissions.Add(submission);
        db.SaveChanges();
    }
}

