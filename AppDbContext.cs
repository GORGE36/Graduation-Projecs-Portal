using System.IO;
using Microsoft.EntityFrameworkCore;

namespace SuperSee;

public class AppDbContext : DbContext
{
    private DbSet<Supervisor> supervisors;

    public DbSet<Supervisor> Supervisors
    {
        get => supervisors;
        set => supervisors = value;
    }

    private DbSet<Student> students;

    public DbSet<Student> Students
    {
        get => students;
        set => students = value;
    }

    private DbSet<Team> teams;

    public DbSet<Team> Teams
    {
        get => teams;
        set => teams = value;
    }

    private DbSet<Project> projects;

    public DbSet<Project> Projects
    {
        get => projects;
        set => projects = value;
    }

    private DbSet<Message> messages;

    public DbSet<Message> Messages
    {
        get => messages;
        set => messages = value;
    }

    private DbSet<StudentCapability> capabilities;

    public DbSet<StudentCapability> Capabilities
    {
        get => capabilities;
        set => capabilities = value;
    }

    private DbSet<Coordinator> coordinators;

    public DbSet<Coordinator> Coordinators
    {
        get => coordinators;
        set => coordinators = value;
    }

    // إضافة جدول رئيس القسم بالطريقة القياسية المختصرة لمنع الـ StackOverflowException
    public DbSet<HeadOfDepartment> HeadOfDepartments { get; set; }

    public DbSet<Task> Tasks { get; set; }
    public DbSet<Milestone> Milestones { get; set; }
    public DbSet<Notification> Notifications { get; set; }

    private DbSet<TeamInvitation> teamInvitations;

    public DbSet<TeamInvitation> TeamInvitations
    {
        get => teamInvitations;
        set => teamInvitations = value;
    }

    private DbSet<FileSubmission> fileSubmissions;

    public DbSet<FileSubmission> FileSubmissions
    {
        get => fileSubmissions;
        set => fileSubmissions = value;
    }

    private DbSet<FileComment> fileComments;

    public DbSet<FileComment> FileComments
    {
        get => fileComments;
        set => fileComments = value;
    }

    public DbSet<CommitteeAvailability> CommitteeAvailabilities { get; set; }
    public DbSet<CommitteeMember> CommitteeMembers { get; set; }

    public AppDbContext() { }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = "supersee.db";
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Many of the issues with FKs in SQLite tests come from the fact that 
        // we're deleting entities that are referenced by others.
        // We'll ensure Cascade delete is set where needed or handle it in tests.

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Coordinator)
            .WithMany()
            .HasForeignKey(t => t.CoordinatorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Supervisor)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.SupervisorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Student>(entity =>
        {
            entity.Property(e => e.StudentName).IsRequired(false);
            entity.HasOne(s => s.Team)
                .WithMany(t => t.Members)
                .HasForeignKey(s => s.TeamId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Project>()
            .HasOne(p => p.Team)
            .WithOne(t => t.Project)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<StudentCapability>(entity =>
        {
            entity.HasKey(c => new { c.StudentId, c.Name });
            entity.HasOne(c => c.Student)
                .WithMany(s => s.Capabilities)
                .HasForeignKey(c => c.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Supervisor>()
            .HasOne(s => s.Coordinator)
            .WithMany(c => c.Supervisors)
            .HasForeignKey(s => s.CoordinatorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.TeamName)
            .IsUnique();

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.MessageId);

            entity.HasOne(m => m.Team)
                .WithMany()
                .HasForeignKey(m => m.TeamId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(m => m.ReceiverSupervisor)
                .WithMany()
                .HasForeignKey(m => m.ReceiverSupervisorId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(t => t.TaskId);
            entity.HasOne(t => t.Team)
                .WithMany()
                .HasForeignKey(t => t.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Milestone>(entity =>
        {
            entity.HasKey(m => m.MilestoneId);
            entity.HasOne(m => m.Task)
                .WithMany(t => t.Milestones)
                .HasForeignKey(m => m.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TeamInvitation>(entity =>
        {
            entity.HasKey(i => i.InvitationId);
            entity.HasOne(i => i.InvitedStudent)
                .WithMany()
                .HasForeignKey(i => i.InvitedStudentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileSubmission>(entity =>
        {
            entity.HasKey(f => f.FileSubmissionId);
            entity.HasOne(f => f.Team)
                .WithMany()
                .HasForeignKey(f => f.TeamId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<FileComment>(entity =>
        {
            entity.HasKey(c => c.FileCommentId);
            entity.HasOne(c => c.FileSubmission)
                .WithMany(f => f.Comments)
                .HasForeignKey(c => c.FileSubmissionId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(c => c.Supervisor)
                .WithMany()
                .HasForeignKey(c => c.SupervisorId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}