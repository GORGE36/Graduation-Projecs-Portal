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

    public AppDbContext() {}

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {}

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
        modelBuilder.Entity<Team>()
            .HasOne(t => t.Coordinator)
            .WithMany()
            .HasForeignKey(t => t.CoordinatorId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasOne(t => t.Supervisor)
            .WithMany(s => s.Teams)
            .HasForeignKey(t => t.SupervisorId)
            .OnDelete(DeleteBehavior.Cascade);
        
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
            .HasOne(t => t.TeamLeader)
            .WithMany()
            .HasForeignKey(t => t.TeamLeaderId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Team>()
            .HasIndex(t => t.TeamName)
            .IsUnique();
    }
}

