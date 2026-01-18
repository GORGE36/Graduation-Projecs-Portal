using SuperSee;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "SuperSee API", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SuperSee API V1");
    c.RoutePrefix = "swagger"; 
});

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    
    if (!db.Coordinators.Any(c => c.CoordinatorName == "Khaldoon"))
    {
        var cood = new Coordinator("Khaldoon", "Khaldoon@gmail.com", "Has00000");
        db.Coordinators.Add(cood);
        db.SaveChanges();
        Console.WriteLine("Coordinator 'Khaldoon' added.");
    }

    var mainCoord = db.Coordinators.First(c => c.CoordinatorName == "Khaldoon");
    
    var supervisorsToAdd = new List<Supervisor>
    {
        new Supervisor("Dr. Ahmed", "ahmed@gmail.com", "hash123", mainCoord.CoordinatorId),
        new Supervisor("Dr. yanal", "Yanal@gmail.com", "hash1233", mainCoord.CoordinatorId),
        new Supervisor("Dr. mohammed", "mohammed@gmail.com", "hash1233", mainCoord.CoordinatorId),
        new Supervisor("Dr. Hamza", "Hamza@gmail.com", "hash1233", mainCoord.CoordinatorId)
    };

    foreach (var sv in supervisorsToAdd)
    {
        if (!db.Supervisors.Any(s => s.SupervisorName == sv.SupervisorName))
        {
            db.Supervisors.Add(sv);
            Console.WriteLine($"Supervisor '{sv.SupervisorName}' added.");
        }
    }

    var studentsToAdd = new List<Student>
    {
        new Student("Zaid Abubaker", "zaid@example.com", "hash-zaid"),
        new Student("Omar Abdullah", "omar@example.com", "hash-omar"),
        new Student("Ahmad Ali", "ahmad@example.com", "hash-ahmad"),
        new Student("Sara Qusay", "sara@example.com", "hash-sara"),
        new Student("ABD ALZOUBI", "ABD@example.com", "hash-abd"),
        new Student("ABDALRAHMAN", "abd@example.com", "hash-abd"),
        new Student(" HAMZAALJ", "HAM@example.com", "hash-ham"),
        new Student("RahmaDaqamsa", "Rhoom@example.com", "hash-rahma"),
        new Student("AhmedDARRAJ", "1ahmed@example.com", "hash-ahmed"),
        new Student("Hala ALmomany ", "hala@example.com", "hash-hala"),
        new Student("YARA ALZOUBI", "yara@example.com", "hash-yara"),
        new Student("kinda ALZOUBI", "kinda@example.com", "hash-kinda"),
        new Student("Rasha ALZOUBI", "rasha@example.com", "hash-rasha"),
        new Student("Mu'min Ali", "Mu'min@example.com", "Mu-rasha"),
        new Student("Faris Hamid", "Faris@example.com", "hash-faris")
    };

    foreach (var st in studentsToAdd)
    {
        if (!db.Students.Any(s => s.StudentName == st.StudentName))
        {
            db.Students.Add(st);
            Console.WriteLine($"Student '{st.StudentName}' added.");
        }
    }

    db.SaveChanges();
    Console.WriteLine("Database initialization complete.");
}

app.UseStaticFiles(); 
app.UseRouting();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Login.html");
    return Task.CompletedTask;
});

app.MapGet("/api/coordinator/available-students", async (AppDbContext db) =>
{
    var students = await db.Students.Where(s => s.TeamId == null).ToListAsync();
    return Results.Ok(students);
});

app.MapPost("/api/login", async (LoginRequest request, AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstOrDefaultAsync(c => c.CoordinatorName == request.Username && c.CoordinatorPasswordHash == request.Password);
    if (coord != null)
    {
        return Results.Ok(new LoginResponse(true, "Coordinator", coord.CoordinatorName, "Dashboord.html", coord.CoordinatorId));
    }
    
    var supervisor = await db.Supervisors.FirstOrDefaultAsync(s => s.SupervisorName == request.Username && s.SupervisorPasswordHash == request.Password);
    if (supervisor != null)
    {
        return Results.Ok(new LoginResponse(true, "Supervisor", supervisor.SupervisorName, "SupervisorDash.html", supervisor.SupervisorId));
    }
    
    var student = await db.Students.FirstOrDefaultAsync(s => s.StudentName == request.Username && s.StudentPasswordHash == request.Password);
    if (student != null)
    {
        return Results.Ok(new LoginResponse(true, "Student", student.StudentName, "StudentDash.html", student.StudentId));
    }

    return Results.Json(new { Success = false, Message = "Invalid username or password" }, statusCode: 401);
});

app.MapGet("/api/supervisor/pending-teams", async (Guid supervisorId, AppDbContext db) =>
{
    var teams = await db.Teams
        .Where(t => t.SupervisorId == supervisorId && t.AssignmentStatus == AssignmentStatus.Pending)
        .Include(t => t.Members)
        .Include(t => t.Project)
        .ToListAsync();
    
    return Results.Ok(teams.Select(t => new {
        t.TeamId,
        t.TeamName,
        t.TeamLeaderId,
        TeamLeaderName = t.Members.FirstOrDefault(m => m.StudentId == t.TeamLeaderId)?.StudentName,
        ProjectTitle = t.Project?.Title,
        ProjectDescription = t.Project?.Description
    }));
});

app.MapPost("/api/supervisor/accept-team/{teamId}", async (Guid teamId, Guid supervisorId, AppDbContext db) =>
{
    var userContext = new UserContext(supervisorId, Role.Supervisor);
    var service = new SupervisorService(db, userContext);
    try
    {
        service.AcceptAssignment(teamId);
        return Results.Ok(new { Success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapPost("/api/supervisor/reject-team/{teamId}", async (Guid teamId, Guid supervisorId, AppDbContext db) =>
{
    var userContext = new UserContext(supervisorId, Role.Supervisor);
    var service = new SupervisorService(db, userContext);
    try
    {
        service.RefuseAssignment(teamId);
        return Results.Ok(new { Success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapGet("/api/coordinator/supervisors", async (AppDbContext db) =>
{
    var supervisors = await db.Supervisors.ToListAsync();
    return Results.Ok(supervisors);
});

app.MapGet("/api/coordinator/teams", async (AppDbContext db) =>
{
    var teams = await db.Teams
        .Include(t => t.Members)
        .Include(t => t.Project)
        .Include(t => t.Supervisor)
        .Select(t => new {
            t.TeamId,
            t.TeamName,
            SupervisorName = t.Supervisor != null ? t.Supervisor.SupervisorName : "Unknown",
            t.AssignmentStatus,
            ProjectTitle = t.Project != null ? t.Project.Title : "No Project",
            Members = t.Members.Select(m => new { m.StudentId, m.StudentName }).ToList()
        })
        .ToListAsync();
    return Results.Ok(teams);
});

app.MapPost("/api/coordinator/swap-members", async (SwapMembersRequest request, AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstAsync();
    var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
    var coordService = new CoordinatorService(db, userContext);

    try
    {
        coordService.SwapMembersBetweenTeams(request.Team1Id, request.Student1Id, request.Team2Id, request.Student2Id);
        return Results.Ok(new { Success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapGet("/api/supervisor/my-teams", async (Guid supervisorId, AppDbContext db) =>
{
    var teams = await db.Teams
        .Where(t => t.SupervisorId == supervisorId && t.AssignmentStatus == AssignmentStatus.Accepted)
        .Include(t => t.Members)
        .Include(t => t.Project)
        .Select(t => new {
            t.TeamId,
            t.TeamName,
            ProjectTitle = t.Project != null ? t.Project.Title : "No Project",
            MembersCount = t.Members.Count,
            TeamLeaderName = t.Members.Where(m => m.StudentId == t.TeamLeaderId).Select(m => m.StudentName).FirstOrDefault()
        })
        .ToListAsync();
    return Results.Ok(teams);
});

app.MapPost("/api/coordinator/create-team", async (SuperSee.DTOs.CreateTeamRequest request, AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstAsync(); 
    var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
    var coordService = new CoordinatorService(db, userContext);

    try
    {
        var team = coordService.CreateTeamWithProject(
            request.SupervisorId,
            request.TeamName,
            request.ProjectTitle,
            request.ProjectDescription,
            request.Deadline == default ? DateTime.Now.AddMonths(6) : request.Deadline,
            request.StudentIds,
            request.TeamLeaderId == default ? request.StudentIds[0] : request.TeamLeaderId
        );
        return Results.Ok(new { Success = true, TeamId = team.TeamId });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapDelete("/api/coordinator/delete-team/{teamId}", async (Guid teamId, AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstAsync();
    var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
    var coordService = new CoordinatorService(db, userContext);

    try
    {
        coordService.DeleteTeam(teamId);
        return Results.Ok(new { Success = true });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

app.MapGet("/api/student/my-team", async (Guid studentId, AppDbContext db) =>
{
    var student = await db.Students
        .Include(s => s.Team)
            .ThenInclude(t => t.Project)
        .Include(s => s.Team)
            .ThenInclude(t => t.Supervisor)
        .Include(s => s.Team)
            .ThenInclude(t => t.Members)
        .FirstOrDefaultAsync(s => s.StudentId == studentId);

    if (student?.Team == null)
    {
        return Results.NotFound(new { Message = "You are not assigned to any team yet." });
    }

    var team = student.Team;
    return Results.Ok(new
    {
        team.TeamId,
        team.TeamName,
        team.AssignmentStatus,
        ProjectTitle = team.Project?.Title,
        ProjectDescription = team.Project?.Description,
        SupervisorName = team.Supervisor?.SupervisorName,
        TeamLeaderId = team.TeamLeaderId,
        Members = team.Members.Select(m => new
        {
            m.StudentId,
            m.StudentName,
            m.StudentEmail
        }).ToList()
    });
});

app.Run();


public record LoginRequest(string Username, string Password);
public record LoginResponse(bool Success, string Role, string Name, string RedirectUrl, Guid? UserId = null);
public record SwapMembersRequest(Guid Team1Id, Guid Student1Id, Guid Team2Id, Guid Student2Id);