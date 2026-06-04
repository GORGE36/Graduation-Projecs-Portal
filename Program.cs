using SuperSee;
using SuperSee.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});

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


    if (!db.HeadOfDepartments.Any(h => h.Name == "Hamzah al-kofahi"))
    {
        var hod = new HeadOfDepartment("Hamzah al-kofahi", "hamzah@gmail.com", "password123");
        db.HeadOfDepartments.Add(hod);
        db.SaveChanges();
        Console.WriteLine("HeadOfDepartment 'Hamzah al-kofahi' added.");
    }
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

    // If this project is started against an older SQLite file that lacks newly added tables
    // (e.g. Messages), create those tables at runtime to avoid "No such table" errors.
    try
    {
        var conn = db.Database.GetDbConnection();
        conn.Open();
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Messages';";
            var result = cmd.ExecuteScalar();
            var exists = Convert.ToInt64(result) > 0;
            if (!exists)
            {
                Console.WriteLine("Messages table missing in DB — creating table now.");
                using var create = conn.CreateCommand();
                create.CommandText = @"CREATE TABLE IF NOT EXISTS Messages (
                    MessageId TEXT NOT NULL PRIMARY KEY,
                    SenderId TEXT NOT NULL,
                    SenderRole INTEGER NOT NULL,
                    TeamId TEXT,
                    ReceiverSupervisorId TEXT,
                    Content TEXT NOT NULL,
                    SentAt TEXT NOT NULL,
                    FileName TEXT,
                    FilePath TEXT
                );";
                create.ExecuteNonQuery();
            }

            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Notifications';";
            result = cmd.ExecuteScalar();
            exists = Convert.ToInt64(result) > 0;
            if (!exists)
            {
                Console.WriteLine("Notifications table missing in DB — creating table now.");
                using var create = conn.CreateCommand();
                create.CommandText = @"CREATE TABLE IF NOT EXISTS Notifications (
                    NotificationId TEXT NOT NULL PRIMARY KEY,
                    UserId TEXT NOT NULL,
                    Message TEXT NOT NULL,
                    Type TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    IsRead INTEGER NOT NULL,
                    RelatedId TEXT
                );";
                create.ExecuteNonQuery();
            }
        }
        // Ensure Tasks and Milestones tables exist in older DB files which may lack them
        using (var cmd2 = conn.CreateCommand())
        {
            cmd2.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Tasks';";
            var res2 = cmd2.ExecuteScalar();
            var tasksExists = Convert.ToInt64(res2) > 0;
            if (!tasksExists)
            {
                Console.WriteLine("Tasks table missing in DB — creating table now.");
                using var createTasks = conn.CreateCommand();
                createTasks.CommandText = @"CREATE TABLE IF NOT EXISTS Tasks (
                    TaskId TEXT NOT NULL PRIMARY KEY,
                    Title TEXT,
                    Description TEXT,
                    Progress INTEGER NOT NULL,
                    TeamId TEXT NOT NULL,
                    CreatedAt TEXT NOT NULL,
                    IsCompleted INTEGER NOT NULL DEFAULT 0,
                    CompletedAt TEXT
                );";
                createTasks.ExecuteNonQuery();
            }
            else
            {
                // Check if IsCompleted and CompletedAt columns exist in Tasks table
                using var checkCols = conn.CreateCommand();
                checkCols.CommandText = "PRAGMA table_info(Tasks);";
                bool isCompletedExists = false;
                bool completedAtExists = false;
                using (var reader = checkCols.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader["name"].ToString();
                        if (name == "IsCompleted") isCompletedExists = true;
                        if (name == "CompletedAt") completedAtExists = true;
                    }
                }

                if (!isCompletedExists)
                {
                    Console.WriteLine("Adding IsCompleted column to Tasks table.");
                    using var addCol = conn.CreateCommand();
                    addCol.CommandText = "ALTER TABLE Tasks ADD COLUMN IsCompleted INTEGER NOT NULL DEFAULT 0;";
                    addCol.ExecuteNonQuery();
                }
                if (!completedAtExists)
                {
                    Console.WriteLine("Adding CompletedAt column to Tasks table.");
                    using var addCol = conn.CreateCommand();
                    addCol.CommandText = "ALTER TABLE Tasks ADD COLUMN CompletedAt TEXT;";
                    addCol.ExecuteNonQuery();
                }
            }
        }

        using (var cmd3 = conn.CreateCommand())
        {
            cmd3.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Milestones';";
            var res3 = cmd3.ExecuteScalar();
            var milesExists = Convert.ToInt64(res3) > 0;
            if (!milesExists)
            {
                Console.WriteLine("Milestones table missing in DB — creating table now.");
                using var createMiles = conn.CreateCommand();
                createMiles.CommandText = @"CREATE TABLE IF NOT EXISTS Milestones (
                    MilestoneId TEXT NOT NULL PRIMARY KEY,
                    Title TEXT,
                    IsCompleted INTEGER NOT NULL,
                    DueDate TEXT,
                    ""Order"" INTEGER NOT NULL DEFAULT 0,
                    TaskId TEXT NOT NULL
                );";
                createMiles.ExecuteNonQuery();
            }
            else
            {
                // Check if 'Order' and 'DueDate' columns exist in Milestones table
                using var checkOrder = conn.CreateCommand();
                checkOrder.CommandText = "PRAGMA table_info(Milestones);";
                bool orderExists = false;
                bool dueDateExists = false;
                using (var reader = checkOrder.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var colName = reader["name"].ToString();
                        if (colName == "Order") orderExists = true;
                        if (colName == "DueDate") dueDateExists = true;
                    }
                }
                if (!orderExists)
                {
                    Console.WriteLine("Adding 'Order' column to Milestones table.");
                    using var addOrder = conn.CreateCommand();
                    addOrder.CommandText = "ALTER TABLE Milestones ADD COLUMN \"Order\" INTEGER NOT NULL DEFAULT 0;";
                    addOrder.ExecuteNonQuery();
                }
                if (!dueDateExists)
                {
                    Console.WriteLine("Adding 'DueDate' column to Milestones table.");
                    using var addDueDate = conn.CreateCommand();
                    addDueDate.CommandText = "ALTER TABLE Milestones ADD COLUMN DueDate TEXT;";
                    addDueDate.ExecuteNonQuery();
                }
            }
        }

        // --- NEW: Committee / اللجنة Table Schema ---
        using (var cmd4 = conn.CreateCommand())
        {
            cmd4.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommitteeAvailabilities';";
            var res4 = cmd4.ExecuteScalar();
            if (Convert.ToInt64(res4) == 0)
            {
                Console.WriteLine("CommitteeAvailabilities table missing — creating now.");
                using var createAvail = conn.CreateCommand();
                createAvail.CommandText = @"CREATE TABLE CommitteeAvailabilities (
                    CommitteeAvailabilityId TEXT NOT NULL PRIMARY KEY,
                    SupervisorId TEXT NOT NULL,
                    AvailableDate TEXT NOT NULL,
                    TimeSlot TEXT NOT NULL
                );";
                createAvail.ExecuteNonQuery();
            }
        }

        using (var cmd5 = conn.CreateCommand())
        {
            cmd5.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='CommitteeMembers';";
            var res5 = cmd5.ExecuteScalar();
            if (Convert.ToInt64(res5) == 0)
            {
                Console.WriteLine("CommitteeMembers table missing — creating now.");
                using var createMem = conn.CreateCommand();
                createMem.CommandText = @"CREATE TABLE CommitteeMembers (
                    CommitteeMemberId TEXT NOT NULL PRIMARY KEY,
                    ProjectId TEXT NOT NULL,
                    SupervisorId TEXT NOT NULL,
                    IsMainSupervisor INTEGER NOT NULL,
                    Grade REAL,
                    GradedAt TEXT
                );";
                createMem.ExecuteNonQuery();
            }
        }

        using (var cmd6 = conn.CreateCommand())
        {
            cmd6.CommandText = "PRAGMA table_info(Projects);";
            bool presentationDateExists = false;
            using (var reader = cmd6.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "PresentationDate")
                    {
                        presentationDateExists = true;
                        break;
                    }
                }
            }
            if (!presentationDateExists)
            {
                Console.WriteLine("Adding PresentationDate column to Projects table.");
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE Projects ADD COLUMN PresentationDate TEXT;";
                addCol.ExecuteNonQuery();
            }
        }

        // --- NEW: Add Description to FileSubmissions ---
        using (var cmdFileDesc = conn.CreateCommand())
        {
            cmdFileDesc.CommandText = "PRAGMA table_info(FileSubmissions);";
            bool descriptionExists = false;
            using (var reader = cmdFileDesc.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals("Description", StringComparison.OrdinalIgnoreCase))
                    {
                        descriptionExists = true;
                        break;
                    }
                }
            }
            if (!descriptionExists)
            {
                Console.WriteLine("Adding Description column to FileSubmissions table.");
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE FileSubmissions ADD COLUMN Description TEXT;";
                addCol.ExecuteNonQuery();
            }
        }

        using (var cmd7 = conn.CreateCommand())
        {
            cmd7.CommandText = "PRAGMA table_info(Teams);";
            bool suggestedSupervisorsExists = false;
            using (var reader = cmd7.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "SuggestedSupervisorIds")
                    {
                        suggestedSupervisorsExists = true;
                        break;
                    }
                }
            }
            if (!suggestedSupervisorsExists)
            {
                Console.WriteLine("Adding SuggestedSupervisorIds column to Teams table.");
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE Teams ADD COLUMN SuggestedSupervisorIds TEXT;";
                addCol.ExecuteNonQuery();
            }

            // --- NEW: Add SupervisorStatus column to Teams table ---
            bool supervisorStatusExists = false;
            cmd7.CommandText = "PRAGMA table_info(Teams);";
            using (var reader = cmd7.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "SupervisorStatus")
                    {
                        supervisorStatusExists = true;
                        break;
                    }
                }
            }
            if (!supervisorStatusExists)
            {
                Console.WriteLine("Adding SupervisorStatus column to Teams table.");
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE Teams ADD COLUMN SupervisorStatus INTEGER NOT NULL DEFAULT 0;";
                addCol.ExecuteNonQuery();
            }

        }

        using (var cmd8 = conn.CreateCommand())
        {
            cmd8.CommandText = "PRAGMA table_info(Messages);";
            bool fileNameExists = false;
            using (var reader = cmd8.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString() == "FileName")
                    {
                        fileNameExists = true;
                        break;
                    }
                }
            }
            if (!fileNameExists)
            {
                Console.WriteLine("Adding FileName and FilePath columns to Messages table.");
                using var addCol1 = conn.CreateCommand();
                addCol1.CommandText = "ALTER TABLE Messages ADD COLUMN FileName TEXT;";
                addCol1.ExecuteNonQuery();
                using var addCol2 = conn.CreateCommand();
                addCol2.CommandText = "ALTER TABLE Messages ADD COLUMN FilePath TEXT;";
                addCol2.ExecuteNonQuery();
            }
        }

        // --- NEW: Add DueDate to Tasks ---
        using (var cmdTaskDueDate = conn.CreateCommand())
        {
            cmdTaskDueDate.CommandText = "PRAGMA table_info(Tasks);";
            bool dueDateExists = false;
            using (var reader = cmdTaskDueDate.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader["name"].ToString().Equals("DueDate", StringComparison.OrdinalIgnoreCase))
                    {
                        dueDateExists = true;
                        break;
                    }
                }
            }
            if (!dueDateExists)
            {
                Console.WriteLine("Adding DueDate column to Tasks table.");
                using var addCol = conn.CreateCommand();
                addCol.CommandText = "ALTER TABLE Tasks ADD COLUMN DueDate TEXT;";
                addCol.ExecuteNonQuery();
            }
        }

        conn.Close();
    }
    catch (Exception ex)
    {
        Console.WriteLine("Warning: failed to ensure database tables or columns exist: " + ex.Message);
    }
}

app.UseStaticFiles(); 
app.UseRouting();

app.MapGet("/", context =>
{
    context.Response.Redirect("/Login.html");
    return System.Threading.Tasks.Task.CompletedTask;
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
    var hod = await db.HeadOfDepartments.FirstOrDefaultAsync(h => h.Name == request.Username && h.PasswordHash == request.Password);
    if (hod != null)
    {
        // نرسل 
        return Results.Ok(new LoginResponse(true, "HeadOfDepartment", hod.Name, "HeadOfDepartment.html", hod.Id));
    }
    return Results.Json(new { Success = false, Message = "Invalid username or password" }, statusCode: 401);
});

app.MapGet("/api/supervisor/pending-teams", async (Guid supervisorId, AppDbContext db) =>
{
    var teams = await db.Teams
        .Where(t => t.SupervisorId == supervisorId && t.AssignmentStatus == AssignmentStatus.Accepted && t.SupervisorStatus == AssignmentStatus.Pending)
        .Include(t => t.Members)
        .Include(t => t.Project)
        .ToListAsync();
    
    return Results.Ok(teams.Select(t => new {
        t.TeamId,
        t.TeamName,
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
    var supervisors = await db.Supervisors
        .Select(s => new { s.SupervisorId, s.SupervisorName })
        .ToListAsync();
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
            t.SupervisorId,
            SupervisorName = t.Supervisor != null ? t.Supervisor.SupervisorName : "Unknown",
            t.AssignmentStatus,
            t.SupervisorStatus,
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
        .Where(t => t.SupervisorId == supervisorId && t.AssignmentStatus == AssignmentStatus.Accepted && t.SupervisorStatus == AssignmentStatus.Accepted)
        .Include(t => t.Members)
        .Include(t => t.Project)
        .Select(t => new {
            t.TeamId,
            t.TeamName,
            ProjectTitle = t.Project != null ? t.Project.Title : "No Project",
            MembersCount = t.Members.Count
        })
        .ToListAsync();
    return Results.Ok(teams);
});

app.MapPost("/api/coordinator/create-team", async (SuperSee.DTOs.CreateTeamRequest request, AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstAsync(); 
    var userContext = new UserContext(coord.CoordinatorId, Role.Coordinator);
    var coordService = new CoordinatorService(db, userContext);

    if (request.Deadline != default && request.Deadline < DateTime.Now)
    {
        return Results.BadRequest(new { Success = false, Message = "Deadline cannot be in the past." });
    }

    try
    {
        var team = coordService.CreateTeamWithProject(
            request.SupervisorId,
            request.TeamName,
            request.ProjectTitle,
            request.ProjectDescription,
            request.Deadline == default ? DateTime.Now.AddMonths(6) : request.Deadline,
            request.StudentIds
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
        Members = team.Members.Select(m => new
        {
            m.StudentId,
            m.StudentName,
            m.StudentEmail
        }).ToList()
    });
});

// Supervisor -> Team: send message to a chosen team
// Updated: accept SenderId from request body (if provided) to avoid brittle query binding.
app.MapPost("/api/supervisor/send-to-team", async (SendMessageToTeamRequest request, Microsoft.AspNetCore.Http.HttpRequest httpReq, AppDbContext db) =>
{
    // Accept SenderId either in the request body (preferred), or as a query parameter or header
    Guid supervisorId = request.SenderId ?? Guid.Empty;

    if (supervisorId == Guid.Empty)
    {
        // try query string
        if (httpReq.Query.TryGetValue("supervisorId", out var qv) && Guid.TryParse(qv.ToString(), out var qguid))
        {
            supervisorId = qguid;
        }
    }

    if (supervisorId == Guid.Empty)
    {
        // try header: X-Supervisor-Id
        if (httpReq.Headers.TryGetValue("X-Supervisor-Id", out var hv) && Guid.TryParse(hv.ToString(), out var hguid))
        {
            supervisorId = hguid;
        }
    }

    if (supervisorId == Guid.Empty)
        return Results.BadRequest(new { Success = false, Message = "SupervisorId is required (body, query string or X-Supervisor-Id header)." });

    var supervisor = await db.Supervisors.FindAsync(supervisorId);
    if (supervisor == null) return Results.BadRequest(new { Success = false, Message = "Supervisor not found." });

    var team = await db.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.TeamId == request.TeamId);
    if (team == null) return Results.BadRequest(new { Success = false, Message = "Team not found." });
    if (team.SupervisorId != supervisorId) return Results.Unauthorized();

    var msg = new Message
    {
        MessageId = Guid.NewGuid(),
        SenderId = supervisorId,
        SenderRole = Role.Supervisor,
        TeamId = request.TeamId,
        Content = request.Content,
        SentAt = DateTime.UtcNow
    };

    db.Messages.Add(msg);
    await db.SaveChangesAsync();

    // Notify team members about the new message from supervisor
    var teamMembers = await db.Students.Where(s => s.TeamId == request.TeamId).ToListAsync();
    foreach (var member in teamMembers)
    {
        db.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = member.StudentId,
            Message = $"New message from supervisor in team chat",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
    }
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true });
});

// Student -> Team/Supervisor: send message to their team, supervisor, or both
app.MapPost("/api/student/send-to-team", async (SendMessageToTeamRequest request, Guid studentId, AppDbContext db) =>
{
    var student = await db.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student == null) return Results.BadRequest(new { Success = false, Message = "Student not found." });
    if (student.TeamId == null) return Results.BadRequest(new { Success = false, Message = "You are not assigned to a team. You cannot send messages or files." });

    bool sendToTeam = request.Target == "Team" || request.Target == "Both";
    bool sendToSupervisor = request.Target == "Supervisor" || request.Target == "Both";

    if (sendToSupervisor && (student.Team.SupervisorId == null || student.Team.SupervisorId == Guid.Empty))
        return Results.BadRequest(new { Success = false, Message = "Team has no supervisor assigned." });

    var msg = new Message
    {
        MessageId = Guid.NewGuid(),
        SenderId = studentId,
        SenderRole = Role.Student,
        TeamId = sendToTeam ? student.TeamId : null,
        ReceiverSupervisorId = sendToSupervisor ? student.Team.SupervisorId : null,
        Content = request.Content,
        SentAt = DateTime.UtcNow
    };

    db.Messages.Add(msg);
    await db.SaveChangesAsync();

    // Notify supervisor
    if (sendToSupervisor && student.Team.SupervisorId.HasValue)
    {
        db.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = student.Team.SupervisorId.Value,
            Message = $"New {(request.Target == "Both" ? "team & direct" : request.Target)} message from {student.StudentName}",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { Success = true });
});

// Student -> Supervisor/Team: send message (can include file)
app.MapPost("/api/student/send-message", async (HttpContext context, Guid studentId, AppDbContext db) =>
{
    var student = await db.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student == null) return Results.BadRequest(new { Success = false, Message = "Student not found." });
    if (student.Team == null) return Results.BadRequest(new { Success = false, Message = "You are not assigned to a team. You cannot send messages or files." });

    var form = await context.Request.ReadFormAsync();
    var content = form["content"].ToString();
    var destination = form["destination"].ToString() ?? "Supervisor";
    var file = form.Files.FirstOrDefault();

    bool sendToTeam = destination == "Team" || destination == "Both";
    bool sendToSupervisor = destination == "Supervisor" || destination == "Both";

    if (sendToSupervisor && (student.Team.SupervisorId == null || student.Team.SupervisorId == Guid.Empty))
        return Results.BadRequest(new { Success = false, Message = "Team has no supervisor assigned." });

    string? fileName = null;
    string? filePath = null;

    if (file != null && file.Length > 0)
    {
        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "messages", student.Team.TeamId.ToString());
        Directory.CreateDirectory(uploadsDir);

        fileName = file.FileName;
        var savedFileName = $"{Guid.NewGuid()}_{fileName}";
        var fullPath = Path.Combine(uploadsDir, savedFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        filePath = $"/uploads/messages/{student.Team.TeamId}/{savedFileName}";
    }

    var msg = new Message
    {
        MessageId = Guid.NewGuid(),
        SenderId = studentId,
        SenderRole = Role.Student,
        TeamId = sendToTeam ? student.TeamId : null,
        ReceiverSupervisorId = sendToSupervisor ? student.Team.SupervisorId : null,
        Content = content,
        FileName = fileName,
        FilePath = filePath,
        SentAt = DateTime.UtcNow
    };

    db.Messages.Add(msg);
    await db.SaveChangesAsync();

    if (sendToSupervisor && student.Team.SupervisorId != null)
    {
        db.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = student.Team.SupervisorId.Value,
            Message = $"New message from {student.StudentName}",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { Success = true });
});

// Get messages relevant to a supervisor (messages sent to their teams or directly to them)
app.MapGet("/api/supervisor/messages", async (Guid supervisorId, AppDbContext db) =>
{
    var msgs = await db.Messages
        .Where(m => m.ReceiverSupervisorId == supervisorId || m.SenderId == supervisorId || (m.Team != null && m.Team.SupervisorId == supervisorId))
        .OrderBy(m => m.SentAt)
        .ToListAsync();

    var result = new List<object>();
    foreach (var m in msgs)
    {
        string senderName = "Unknown";
        if (m.SenderRole == Role.Supervisor)
        {
            var s = await db.Supervisors.FindAsync(m.SenderId);
            if (s != null) senderName = s.SupervisorName;
        }
        else if (m.SenderRole == Role.Student)
        {
            var s = await db.Students.FindAsync(m.SenderId);
            if (s != null) senderName = s.StudentName;
        }
        else if (m.SenderRole == Role.Coordinator)
        {
            var c = await db.Coordinators.FindAsync(m.SenderId);
            if (c != null) senderName = c.CoordinatorName;
        }

        result.Add(new {
            m.MessageId,
            m.SenderId,
            m.SenderRole,
            SenderName = senderName,
            m.TeamId,
            m.ReceiverSupervisorId,
            m.Content,
            m.FileName,
            m.FilePath,
            m.SentAt
        });
    }

    return Results.Ok(result);
});

// Coordinator -> Supervisor: send message to a specific supervisor
app.MapPost("/api/coordinator/send-to-supervisor", async (SendMessageToSupervisorRequest request, AppDbContext db) =>
{
    var msg = new Message
    {
        MessageId = Guid.NewGuid(),
        SenderId = request.SenderId ?? Guid.Empty,
        SenderRole = Role.Coordinator,
        ReceiverSupervisorId = request.SupervisorId,
        Content = request.Content,
        SentAt = DateTime.UtcNow
    };

    db.Messages.Add(msg);
    
    db.Notifications.Add(new Notification
    {
        NotificationId = Guid.NewGuid(),
        UserId = request.SupervisorId,
        Message = "لديك رسالة جديدة من المنسق",
        Type = "NewMessage",
        RelatedId = msg.MessageId.ToString(),
        CreatedAt = DateTime.UtcNow
    });

    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

// Coordinator -> All Supervisors: send message to all supervisors
app.MapPost("/api/coordinator/send-to-all-supervisors", async (SendMessageToAllSupervisorsRequest request, AppDbContext db) =>
{
    var supervisors = await db.Supervisors.ToListAsync();
    foreach (var sup in supervisors)
    {
        var msg = new Message
        {
            MessageId = Guid.NewGuid(),
            SenderId = request.SenderId ?? Guid.Empty,
            SenderRole = Role.Coordinator,
            ReceiverSupervisorId = sup.SupervisorId,
            Content = request.Content,
            SentAt = DateTime.UtcNow
        };
        db.Messages.Add(msg);

        db.Notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = sup.SupervisorId,
            Message = "رسالة عامة جديدة من المنسق لكافة المشرفين",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow
        });
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});


// Get messages between coordinator and a specific supervisor
app.MapGet("/api/coordinator/supervisor-messages", async (Guid supervisorId, AppDbContext db) =>
{
    var msgs = await db.Messages
        .Where(m => (m.ReceiverSupervisorId == supervisorId && m.SenderRole == Role.Coordinator) || 
                    (m.SenderId == supervisorId && m.SenderRole == Role.Supervisor))
        .OrderBy(m => m.SentAt)
        .ToListAsync();

    var result = new List<object>();
    foreach (var m in msgs)
    {
        string senderName = "Unknown";
        if (m.SenderRole == Role.Coordinator)
        {
            var c = await db.Coordinators.FindAsync(m.SenderId);
            if (c != null) senderName = c.CoordinatorName;
        }
        else if (m.SenderRole == Role.Supervisor)
        {
            var s = await db.Supervisors.FindAsync(m.SenderId);
            if (s != null) senderName = s.SupervisorName;
        }

        result.Add(new {
            m.MessageId,
            m.SenderId,
            m.SenderRole,
            SenderName = senderName,
            m.ReceiverSupervisorId,
            m.Content,
            m.SentAt
        });
    }

    return Results.Ok(result);
});

// Coordinator -> Team: send message to a chosen team
app.MapPost("/api/coordinator/send-to-team", async (SendMessageToTeamRequest request, Microsoft.AspNetCore.Http.HttpRequest httpReq, AppDbContext db) =>
{
    Guid coordinatorId = request.SenderId ?? Guid.Empty;

    if (coordinatorId == Guid.Empty)
    {
        if (httpReq.Query.TryGetValue("coordinatorId", out var qv) && Guid.TryParse(qv.ToString(), out var qguid))
        {
            coordinatorId = qguid;
        }
    }

    if (coordinatorId == Guid.Empty)
    {
        if (httpReq.Headers.TryGetValue("X-Coordinator-Id", out var hv) && Guid.TryParse(hv.ToString(), out var hguid))
        {
            coordinatorId = hguid;
        }
    }

    if (coordinatorId == Guid.Empty)
        return Results.BadRequest(new { Success = false, Message = "CoordinatorId is required." });

    var coordinator = await db.Coordinators.FindAsync(coordinatorId);
    if (coordinator == null) return Results.BadRequest(new { Success = false, Message = "Coordinator not found." });

    var team = await db.Teams.Include(t => t.Members).FirstOrDefaultAsync(t => t.TeamId == request.TeamId);
    if (team == null) return Results.BadRequest(new { Success = false, Message = "Team not found." });

    var msg = new Message
    {
        MessageId = Guid.NewGuid(),
        SenderId = coordinatorId,
        SenderRole = Role.Coordinator,
        TeamId = request.TeamId,
        Content = request.Content,
        SentAt = DateTime.UtcNow
    };

    db.Messages.Add(msg);
    await db.SaveChangesAsync();

    // Notify team members and supervisor
    var notifications = new List<Notification>();
    
    // Notify students
    foreach (var member in team.Members)
    {
        notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = member.StudentId,
            Message = $"New message from Coordinator in team chat",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
    }

    // Notify supervisor
    if (team.SupervisorId != null)
    {
        notifications.Add(new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = team.SupervisorId.Value,
            Message = $"New message from Coordinator for team {team.TeamName}",
            Type = "NewMessage",
            RelatedId = msg.MessageId.ToString(),
            CreatedAt = DateTime.UtcNow,
            IsRead = false
        });
    }

    db.Notifications.AddRange(notifications);
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true });
});

// Get messages for a specific team (useful for coordinator to see chat history)
app.MapGet("/api/coordinator/team-messages", async (Guid teamId, AppDbContext db) =>
{
    var msgs = await db.Messages
        .Where(m => m.TeamId == teamId)
        .OrderBy(m => m.SentAt)
        .ToListAsync();

    var result = new List<object>();
    foreach (var m in msgs)
    {
        string senderName = "Unknown";
        if (m.SenderRole == Role.Supervisor)
        {
            var s = await db.Supervisors.FindAsync(m.SenderId);
            if (s != null) senderName = s.SupervisorName;
        }
        else if (m.SenderRole == Role.Student)
        {
            var s = await db.Students.FindAsync(m.SenderId);
            if (s != null) senderName = s.StudentName;
        }
        else if (m.SenderRole == Role.Coordinator)
        {
            var c = await db.Coordinators.FindAsync(m.SenderId);
            if (c != null) senderName = c.CoordinatorName;
        }

        result.Add(new {
            m.MessageId,
            m.SenderId,
            m.SenderRole,
            SenderName = senderName,
            m.TeamId,
            m.Content,
            m.SentAt
        });
    }

    return Results.Ok(result);
});

// Student: request to create a new team (sent to coordinator for approval)
app.MapPost("/api/student/request-team", async (CreateTeamRequestByStudent request, Guid studentId, AppDbContext db) =>
{
    if (request.Deadline.HasValue && request.Deadline.Value < DateTime.Now)
    {
        return Results.BadRequest(new { Success = false, Message = "Project deadline cannot be in the past." });
    }

    // basic validation
    if (request.StudentIds == null || request.StudentIds.Count < 1 || request.StudentIds.Count > 5)
        return Results.BadRequest(new { Success = false, Message = "A team must have between 1 and 5 members." });

    if (request.SuggestedSupervisorIds == null || request.SuggestedSupervisorIds.Count != 3)
        return Results.BadRequest(new { Success = false, Message = "You must select exactly 3 supervisors." });

    // ensure all students exist and are not already in a team (pending or accepted)
    var students = db.Students.Where(s => request.StudentIds.Contains(s.StudentId)).ToList();
    if (students.Count != request.StudentIds.Count)
        return Results.BadRequest(new { Success = false, Message = "One or more students not found." });
    if (students.Any(s => s.TeamId != null))
    {
        var alreadyInTeam = students.First(s => s.TeamId != null);
        return Results.BadRequest(new { Success = false, Message = $"Student '{alreadyInTeam.StudentName}' is already assigned to a team (either pending or accepted)." });
    }

    // create team and project, mark as pending coordinator approval
    var team = new Team(request.TeamName, null)
    {
        CoordinatorId = null,
        AssignmentStatus = AssignmentStatus.Pending,
        SuggestedSupervisorIds = string.Join(",", request.SuggestedSupervisorIds)
    };

    foreach (var s in students)
    {
        s.Team = team;
        s.TeamId = team.TeamId;
        team.Members.Add(s);
    }

    var project = new Project(request.ProjectTitle, request.ProjectDescription, request.Deadline == null ? DateTime.Now.AddMonths(6) : request.Deadline.Value, team.TeamId);
    team.Project = project;

    db.Teams.Add(team);
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true, TeamId = team.TeamId });
});

// Coordinator: list student-created pending team requests
app.MapGet("/api/coordinator/supervisor-workload", async (AppDbContext db) =>
{
    try
    {
        var supervisors = await db.Supervisors
            .Include(s => s.Teams)
                .ThenInclude(t => t.Members)
            .Include(s => s.Teams)
                .ThenInclude(t => t.Project)
            .ToListAsync();

        var result = supervisors.Select(s => new
        {
            s.SupervisorId,
            s.SupervisorName,
            TeamCount = (s.Teams ?? new List<Team>()).Count(t => t.AssignmentStatus != AssignmentStatus.Refused),
            Teams = (s.Teams ?? new List<Team>()).Where(t => t.AssignmentStatus != AssignmentStatus.Refused).Select(t => new
            {
                t.TeamId,
                t.TeamName,
                ProjectTitle = t.Project != null ? (t.Project.Title ?? "بدون عنوان") : "بدون عنوان",
                Students = (t.Members ?? new List<Student>()).Select(m => m.StudentName).ToList()
            }).ToList()
        }).ToList();

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/api/coordinator/pending-requests", async (AppDbContext db) =>
{
    var teams = await db.Teams
        .Where(t => t.AssignmentStatus == AssignmentStatus.Pending)
        .Include(t => t.Members)
        .Include(t => t.Project)
        .ToListAsync();

    return Results.Ok(teams.Select(t => {
        var suggestedIdsStr = (t.SuggestedSupervisorIds ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries);
        var suggestedGuids = suggestedIdsStr.Select(id => Guid.TryParse(id, out var g) ? g : Guid.Empty).Where(g => g != Guid.Empty).ToList();
        
        var supervisorsMap = db.Supervisors
            .Where(s => suggestedGuids.Contains(s.SupervisorId))
            .Select(s => new { s.SupervisorId, s.SupervisorName })
            .ToDictionary(s => s.SupervisorId);

        var suggestedSups = suggestedGuids
            .Where(id => supervisorsMap.ContainsKey(id))
            .Select(id => supervisorsMap[id])
            .ToList();

        return new {
            t.TeamId,
            t.TeamName,
            Members = t.Members.Select(m => new { m.StudentId, m.StudentName }).ToList(),
            ProjectTitle = t.Project?.Title,
            ProjectDescription = t.Project?.Description,
            SuggestedSupervisors = suggestedSups
        };
    }));
});

// Coordinator: decide on a student-created request (accept -> assign supervisor, or refuse)
app.MapPost("/api/coordinator/decide-request/{teamId}", async (Guid teamId, DecideRequestRequest request, Guid coordinatorId, AppDbContext db) =>
{
    var coord = await db.Coordinators.FindAsync(coordinatorId);
    if (coord == null) return Results.BadRequest(new { Success = false, Message = "Coordinator not found." });

    var team = await db.Teams.FindAsync(teamId);
    if (team == null) return Results.BadRequest(new { Success = false, Message = "Team not found." });
    if (team.AssignmentStatus != AssignmentStatus.Pending) return Results.BadRequest(new { Success = false, Message = "Team is not a pending coordinator request." });

    if (request.Accept)
    {
        if (request.SupervisorId == null || request.SupervisorId == Guid.Empty) return Results.BadRequest(new { Success = false, Message = "SupervisorId required when accepting." });
        var sup = await db.Supervisors.FindAsync(request.SupervisorId.Value);
        if (sup == null) return Results.BadRequest(new { Success = false, Message = "Supervisor not found." });

        // Constraint: At most each supervisor can have 5 teams
        int activeTeamCount = await db.Teams.CountAsync(t => t.SupervisorId == request.SupervisorId.Value && t.AssignmentStatus != AssignmentStatus.Refused);
        if (activeTeamCount >= 5)
        {
            return Results.BadRequest(new { Success = false, Message = $"Supervisor {sup.SupervisorName} already has the maximum of 5 teams." });
        }

        team.SupervisorId = request.SupervisorId.Value;
        // set Accepted status so it appears in "Created Teams" and disappears from "Pending Requests"
        team.AssignmentStatus = AssignmentStatus.Accepted;
        // Mark supervisor status as pending for the assigned supervisor
        team.SupervisorStatus = AssignmentStatus.Pending;
        
        // Notify members
        var members = await db.Students.Where(s => s.TeamId == teamId).ToListAsync();
        foreach (var student in members)
        {
            db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = student.StudentId,
                Message = $"Your team '{team.TeamName}' has been approved by the Coordinator and assigned to supervisor {sup.SupervisorName}. Waiting for supervisor acceptance.",
                Type = "TeamApproved",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
        
        await db.SaveChangesAsync();
        return Results.Ok(new { Success = true });
    }
    else
    {
        var userContext = new UserContext(coordinatorId, Role.Coordinator);
        var service = new CoordinatorService(db, userContext);
        service.RejectTeamByCoordinator(teamId);
        return Results.Ok(new { Success = true });
    }
});

app.MapGet("/api/hod/stats", async (Guid hodId, AppDbContext db) =>
{
    var countSup = await db.Supervisors.CountAsync();
    var countProj = await db.Projects.CountAsync();
    var countMyTeams = await db.Teams.CountAsync(t => t.SupervisorId == hodId);
    var countCommittees = await db.CommitteeMembers.CountAsync(cm => cm.SupervisorId == hodId);

    return Results.Ok(new {
        countSup,
        countProj,
        countMyTeams,
        countCommittees
    });
});

app.MapGet("/api/hod/supervisors", async (AppDbContext db) =>
{
    var supervisors = await db.Supervisors
        .Select(s => new {
            s.SupervisorId,
            s.SupervisorName,
            TeamCount = db.Teams.Count(t => t.SupervisorId == s.SupervisorId),
            Status = "نشط" // Simplification for now
        })
        .ToListAsync();
    return Results.Ok(supervisors);
});

app.MapGet("/api/hod/teams-progress", async (AppDbContext db) =>
{
    var teams = await db.Teams
        .Include(t => t.Project)
        .Include(t => t.Supervisor)
        .Select(t => new {
            TeamName = t.TeamName,
            ProjectTitle = t.Project != null ? t.Project.Title : "No Project",
            SupervisorName = t.Supervisor != null ? t.Supervisor.SupervisorName : "No Supervisor",
            Progress = db.Tasks.Where(task => task.TeamId == t.TeamId).Any() 
                       ? db.Tasks.Where(task => task.TeamId == t.TeamId).Average(task => (double)task.Progress) 
                       : 0,
            CurrentPhase = db.Tasks.Where(task => task.TeamId == t.TeamId && !task.IsCompleted)
                             .OrderByDescending(task => task.CreatedAt)
                             .Select(task => task.Title)
                             .FirstOrDefault() ?? "قيد البدء"
        })
        .ToListAsync();
    return Results.Ok(teams);
});

app.MapGet("/api/hod/coordinator-status", async (AppDbContext db) =>
{
    var coord = await db.Coordinators.FirstOrDefaultAsync();
    if (coord == null) return Results.NotFound();

    return Results.Ok(new {
        Name = coord.CoordinatorName,
        LastAction = "تم الانتهاء من هيكلة وتوزيع مقيمي لجان المناقشة للمجموعات.", // Mock for now
        Status = "متابع إدارياً"
    });
});

app.MapPost("/api/hod/broadcast", async (BroadcastDirectiveRequest request, Guid hodId, AppDbContext db) =>
{
    var hod = await db.HeadOfDepartments.FindAsync(hodId);
    if (hod == null) return Results.BadRequest();

    var content = $"[توجيه رسمي من رئيس القسم]: {request.Content}";
    
    if (request.Target == "all" || request.Target == "coord")
    {
        var coord = await db.Coordinators.FirstOrDefaultAsync();
        if (coord != null)
        {
            db.Notifications.Add(new Notification {
                NotificationId = Guid.NewGuid(),
                UserId = coord.CoordinatorId,
                Message = content,
                Type = "Directive",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
    }

    if (request.Target == "all" || request.Target == "sups")
    {
        var supervisors = await db.Supervisors.ToListAsync();
        foreach (var sup in supervisors)
        {
            db.Notifications.Add(new Notification {
                NotificationId = Guid.NewGuid(),
                UserId = sup.SupervisorId,
                Message = content,
                Type = "Directive",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

app.MapPost("/api/coordinator/broadcast", async (BroadcastDirectiveRequest request, Guid coordinatorId, AppDbContext db) =>
{
    var coord = await db.Coordinators.FindAsync(coordinatorId);
    if (coord == null) return Results.BadRequest();

    var content = $"[رسالة من منسق القسم]: {request.Content}";

    if (request.Target == "all" || request.Target == "hod")
    {
        var hods = await db.HeadOfDepartments.ToListAsync();
        foreach (var hod in hods)
        {
            db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = hod.Id,
                Message = content,
                Type = "CoordinatorMessage",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
    }

    if (request.Target == "all" || request.Target == "sups")
    {
        var supervisors = await db.Supervisors.ToListAsync();
        foreach (var sup in supervisors)
        {
            db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = sup.SupervisorId,
                Message = content,
                Type = "CoordinatorMessage",
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
    }

    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

// Get messages for a student's team
app.MapGet("/api/student/messages", async (Guid studentId, AppDbContext db) =>
{
    var student = await db.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student == null) return Results.BadRequest(new { Success = false, Message = "Student not found." });
    if (student.TeamId == null) return Results.BadRequest(new { Success = false, Message = "You are not assigned to a team. You cannot send messages or files." });

    var teamId = student.TeamId.Value;
    var msgs = await db.Messages
        .Where(m => m.TeamId == teamId || (m.ReceiverSupervisorId == student.Team.SupervisorId && m.SenderId == studentId) || (m.ReceiverSupervisorId == null && m.SenderId == student.Team.SupervisorId && m.TeamId == null))
        .OrderBy(m => m.SentAt)
        .ToListAsync();

    var result = new List<object>();
    foreach (var m in msgs)
    {
        string senderName = "Unknown";
        if (m.SenderRole == Role.Supervisor)
        {
            var s = await db.Supervisors.FindAsync(m.SenderId);
            if (s != null) senderName = s.SupervisorName;
        }
        else if (m.SenderRole == Role.Student)
        {
            var s = await db.Students.FindAsync(m.SenderId);
            if (s != null) senderName = s.StudentName;
        }
        else if (m.SenderRole == Role.Coordinator)
        {
            var c = await db.Coordinators.FindAsync(m.SenderId);
            if (c != null) senderName = c.CoordinatorName;
        }

        result.Add(new {
            m.MessageId,
            m.SenderId,
            m.SenderRole,
            SenderName = senderName,
            m.TeamId,
            m.ReceiverSupervisorId,
            m.Content,
            m.FileName,
            m.FilePath,
            m.SentAt
        });
    }

    return Results.Ok(result);
});

// Tasks & Milestones Endpoints
app.MapGet("/api/supervisor/teams/{teamId}/tasks", async (Guid teamId, AppDbContext db) =>
{
    var tasks = await db.Tasks
        .Where(t => t.TeamId == teamId)
        .Include(t => t.Milestones)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    foreach (var taskItem in tasks)
    {
        taskItem.Milestones = taskItem.Milestones.OrderBy(m => m.Order).ToList();
    }

    return Results.Ok(tasks);
});

app.MapPost("/api/supervisor/tasks", async (CreateTaskRequest request, AppDbContext db, ILogger<Program> logger) =>
{
    if (string.IsNullOrWhiteSpace(request.Title))
        return Results.BadRequest(new { error = "Title is required" });

    if (request.TeamId == Guid.Empty)
        return Results.BadRequest(new { error = "TeamId is required" });

    if (!await db.Teams.AnyAsync(t => t.TeamId == request.TeamId))
        return Results.BadRequest(new { error = $"Team {request.TeamId} not found" });

    var task = new SuperSee.Task
    {
        TaskId = Guid.NewGuid(),
        TeamId = request.TeamId,
        Title = request.Title,
        Description = request.Description ?? string.Empty,
        Progress = request.InitialProgress,
        CreatedAt = DateTime.UtcNow,
        DueDate = request.DueDate
    };

    if (request.Milestones != null)
    {
        int order = 0;
        foreach (var mTitle in request.Milestones)
        {
            if (string.IsNullOrWhiteSpace(mTitle)) continue;
            task.Milestones.Add(new Milestone
            {
                MilestoneId = Guid.NewGuid(),
                Title = mTitle,
                IsCompleted = false,
                Order = order++
            });
        }
    }

    try
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync();

        // Notify all team members about the new task
        var teamMembers = await db.Students.Where(s => s.TeamId == request.TeamId).ToListAsync();
        foreach (var member in teamMembers)
        {
            db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = member.StudentId,
                Message = $"New task assigned: {task.Title}",
                Type = "NewTask",
                RelatedId = task.TaskId.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
        await db.SaveChangesAsync();

        return Results.Ok(task);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create task for team {TeamId}", request.TeamId);
        return Results.Problem(
            detail: ex.InnerException?.Message ?? ex.Message,
            statusCode: 500,
            title: "Failed to create task");
    }
});

app.MapDelete("/api/supervisor/tasks/{taskId}", async (Guid taskId, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(taskId);
    if (task == null) return Results.NotFound();

    db.Tasks.Remove(task);
    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

app.MapPost("/api/supervisor/tasks/{taskId}/progress", async (Guid taskId, UpdateProgressRequest request, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(taskId);
    if (task == null) return Results.NotFound();

    task.Progress = request.Progress;
    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

app.MapPost("/api/supervisor/milestones/{milestoneId}/toggle", async (Guid milestoneId, AppDbContext db) =>
{
    var milestone = await db.Milestones.FindAsync(milestoneId);
    if (milestone == null) return Results.NotFound();

    milestone.IsCompleted = !milestone.IsCompleted;
    await db.SaveChangesAsync();

    // After toggling milestone, check if all milestones in the same task are completed
    var taskId = milestone.TaskId;
    var allMilestones = await db.Milestones.Where(m => m.TaskId == taskId).ToListAsync();
    var task = await db.Tasks.FindAsync(taskId);

    if (task != null)
    {
        bool allDone = allMilestones.All(m => m.IsCompleted);
        if (allDone && allMilestones.Any()) // Only complete if there's at least one milestone
        {
            task.IsCompleted = true;
            task.CompletedAt = DateTime.UtcNow;
            task.Progress = 100;
        }
        else
        {
            // If at least one is not done, task is not completed
            task.IsCompleted = false;
            task.CompletedAt = null;
            // Optionally update progress based on completed milestones
            if (allMilestones.Any())
            {
                task.Progress = (int)((double)allMilestones.Count(m => m.IsCompleted) / allMilestones.Count * 100);
            }
        }
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { Success = true, IsCompleted = milestone.IsCompleted, TaskCompleted = task?.IsCompleted ?? false });
});

app.MapPost("/api/supervisor/milestones/{milestoneId}/due-date", async (Guid milestoneId, UpdateMilestoneDueDateRequest request, AppDbContext db) =>
{
    if (request.DueDate < DateTime.Now)
    {
        return Results.BadRequest(new { Success = false, Message = "Due date cannot be in the past." });
    }

    var milestone = await db.Milestones.FindAsync(milestoneId);
    if (milestone == null) return Results.NotFound();

    milestone.DueDate = request.DueDate;
    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

// Email validation endpoint: check if student exists by email
app.MapGet("/api/students/by-email", async (string email, AppDbContext db) =>
{
    var student = await db.Students.FirstOrDefaultAsync(s => s.StudentEmail == email);
    if (student == null)
        return Results.NotFound(new { Success = false, Message = "Student not found." });

    return Results.Ok(new { 
        Success = true, 
        StudentId = student.StudentId, 
        StudentName = student.StudentName,
        StudentEmail = student.StudentEmail
    });
});

// Student: invite teammates by name (creates pending invitations)
app.MapPost("/api/student/invite-to-team", async (InviteToTeamRequest request, Guid studentId, AppDbContext db) =>
{
    // Validate names list is not empty
    if (request.InvitedNames == null || request.InvitedNames.Count == 0)
        return Results.BadRequest(new { Success = false, Message = "No names provided." });

    if (request.InvitedNames.Count > 4) // max 5 total (current user + 4 others)
        return Results.BadRequest(new { Success = false, Message = "Team cannot exceed 5 members." });

    // Verify all names exist and get student IDs
    var invitedStudents = new List<Student>();
    foreach (var name in request.InvitedNames)
    {
        var student = await db.Students.FirstOrDefaultAsync(s => s.StudentName == name);
        if (student == null)
            return Results.BadRequest(new { Success = false, Message = $"Student with name '{name}' not found in the database." });

        if (student.StudentId == studentId)
            return Results.BadRequest(new { Success = false, Message = $"You cannot invite yourself." });

        if (student.TeamId != null)
            return Results.BadRequest(new { Success = false, Message = $"Student '{name}' is already in a team." });

        invitedStudents.Add(student);
    }

    // Create temporary TeamRequestId to group invitations (not yet a real Team)
    var teamRequestId = Guid.NewGuid();

    // Create invitations for all invited students
    foreach (var student in invitedStudents)
    {
        var invitation = new TeamInvitation
        {
            InvitationId = Guid.NewGuid(),
            TeamRequestId = teamRequestId,
            InvitedStudentId = student.StudentId,
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        db.TeamInvitations.Add(invitation);
    }

    // Also add the team leader (current student) as accepted
    var leaderInvitation = new TeamInvitation
    {
        InvitationId = Guid.NewGuid(),
        TeamRequestId = teamRequestId,
        InvitedStudentId = studentId,
        Status = InvitationStatus.Accepted,
        CreatedAt = DateTime.UtcNow,
        RespondedAt = DateTime.UtcNow
    };
    db.TeamInvitations.Add(leaderInvitation);

    // Store request metadata in a temporary location or return it for client-side storage
    await db.SaveChangesAsync();

    return Results.Ok(new { 
        Success = true, 
        TeamRequestId = teamRequestId,
        Message = "Invitations sent. Waiting for teammates to accept."
    });
});

// Student: list pending invitations for the current student
app.MapGet("/api/student/pending-invitations", async (Guid studentId, AppDbContext db) =>
{
    var invitations = await db.TeamInvitations
        .Where(i => i.InvitedStudentId == studentId && i.Status == InvitationStatus.Pending)
        .OrderByDescending(i => i.CreatedAt)
        .Select(i => new {
            i.InvitationId,
            i.TeamRequestId,
            i.Status,
            i.CreatedAt,
            InvitationCount = db.TeamInvitations.Count(ti => ti.TeamRequestId == i.TeamRequestId)
        })
        .ToListAsync();

    return Results.Ok(invitations);
});

// Student: accept an invitation
app.MapPost("/api/student/accept-invitation/{invitationId}", async (Guid invitationId, Guid studentId, AppDbContext db) =>
{
    var invitation = await db.TeamInvitations.FirstOrDefaultAsync(i => i.InvitationId == invitationId);
    if (invitation == null)
        return Results.NotFound(new { Success = false, Message = "Invitation not found." });

    if (invitation.InvitedStudentId != studentId)
        return Results.Json(new { Success = false, Message = "You cannot accept this invitation." }, statusCode: 401);

    if (invitation.Status != InvitationStatus.Pending)
        return Results.BadRequest(new { Success = false, Message = "Invitation has already been responded to." });

    invitation.Status = InvitationStatus.Accepted;
    invitation.RespondedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true });
});

// Student: reject an invitation
app.MapPost("/api/student/reject-invitation/{invitationId}", async (Guid invitationId, Guid studentId, AppDbContext db) =>
{
    var invitation = await db.TeamInvitations.FirstOrDefaultAsync(i => i.InvitationId == invitationId);
    if (invitation == null)
        return Results.NotFound(new { Success = false, Message = "Invitation not found." });

    if (invitation.InvitedStudentId != studentId)
        return Results.Json(new { Success = false, Message = "You cannot reject this invitation." }, statusCode: 401);

    if (invitation.Status != InvitationStatus.Pending)
        return Results.BadRequest(new { Success = false, Message = "Invitation has already been responded to." });

    invitation.Status = InvitationStatus.Rejected;
    invitation.RespondedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    // Check if all other invitations for this TeamRequestId are now rejected or not all accepted
    var teamRequestInvitations = await db.TeamInvitations
        .Where(i => i.TeamRequestId == invitation.TeamRequestId)
        .ToListAsync();

    // If any invitation is rejected, cancel the entire team request
    if (teamRequestInvitations.Any(i => i.Status == InvitationStatus.Rejected))
    {
        db.TeamInvitations.RemoveRange(teamRequestInvitations);
        await db.SaveChangesAsync();
        return Results.Ok(new { 
            Success = true, 
            Message = "Team request has been cancelled due to rejection." 
        });
    }

    return Results.Ok(new { Success = true });
});

// Student: finalize team creation after more than 1 member accepts the invitation
app.MapPost("/api/student/finalize-team-request", async (FinalizeTeamRequestRequest request, Guid studentId, AppDbContext db) =>
{
    // Verify all invitations for this request are responded to
    var invitations = await db.TeamInvitations
        .Where(i => i.TeamRequestId == request.TeamRequestId)
        .ToListAsync();

    if (invitations.Count == 0)
        return Results.BadRequest(new { Success = false, Message = "Team request not found." });

    // Count accepted invitations (must have more than just the leader)
    var acceptedCount = invitations.Count(i => i.Status == InvitationStatus.Accepted);
    if (acceptedCount < 2)
        return Results.BadRequest(new { Success = false, Message = "At least 2 members (including you) must accept to create a team." });

    // Get all accepted student IDs
    var studentIds = invitations.Where(i => i.Status == InvitationStatus.Accepted).Select(i => i.InvitedStudentId).ToList();

    // Create actual Team with Project (marked as Pending for Coordinator approval)
    var team = new Team(request.TeamName, null)
    {
        CoordinatorId = null,
        AssignmentStatus = AssignmentStatus.Pending
    };

    // Fetch students and add to team
    var students = await db.Students.Where(s => studentIds.Contains(s.StudentId)).ToListAsync();
    foreach (var student in students)
    {
        student.Team = team;
        student.TeamId = team.TeamId;
        team.Members.Add(student);
    }

    var project = new Project(
        request.ProjectTitle, 
        request.ProjectDescription, 
        request.Deadline ?? DateTime.Now.AddMonths(6), 
        team.TeamId
    );
    team.Project = project;

    db.Teams.Add(team);

    // Delete old invitations as they're now replaced by Team assignment
    db.TeamInvitations.RemoveRange(invitations);

    await db.SaveChangesAsync();

    return Results.Ok(new { 
        Success = true, 
        TeamId = team.TeamId, 
        Message = "Team created and sent to Coordinator for approval." 
    });
});

// File Submission: Student uploads file for their team
app.MapPost("/api/student/submit-file", async (HttpContext context, Guid studentId, AppDbContext db) =>
{
    try
    {
        var student = await db.Students.Include(s => s.Team).ThenInclude(t => t.Project).FirstOrDefaultAsync(s => s.StudentId == studentId);
        if (student?.Team == null)
            return Results.BadRequest(new { Success = false, Message = "You are not assigned to a team. You cannot send messages or files." });

        var form = await context.Request.ReadFormAsync();
        var file = form.Files.FirstOrDefault();
        if (file == null || file.Length == 0)
            return Results.BadRequest(new { Success = false, Message = "No file provided." });

        var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", student.Team.TeamId.ToString());
        Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}_{file.FileName}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var submission = new FileSubmission
        {
            FileSubmissionId = Guid.NewGuid(),
            TeamId = student.Team.TeamId,
            FileName = file.FileName,
            Description = form["description"].ToString() ?? string.Empty,
            FilePath = $"/uploads/{student.Team.TeamId}/{fileName}",
            SubmittedAt = DateTime.UtcNow,
            IsLate = DateTime.UtcNow > student.Team.Project?.Deadline
        };

        db.FileSubmissions.Add(submission);
        await db.SaveChangesAsync();

        return Results.Ok(new { Success = true, FileSubmissionId = submission.FileSubmissionId, FileName = file.FileName });
    }
    catch (Exception ex)
    {
        return Results.BadRequest(new { Success = false, Message = ex.Message });
    }
});

// Student: Get their team's files
app.MapGet("/api/student/files", async (Guid studentId, AppDbContext db) =>
{
    var student = await db.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student?.Team == null)
        return Results.BadRequest(new { Success = false, Message = "Student not found or not in a team." });

    var files = await db.FileSubmissions
        .Where(f => f.TeamId == student.TeamId)
        .OrderByDescending(f => f.SubmittedAt)
        .Select(f => new {
            f.FileSubmissionId,
            f.FileName,
            FilePath = f.FilePath,
            FileUrl = f.FilePath, // For frontend compatibility
            UploadedAt = f.SubmittedAt
        })
        .ToListAsync();

    return Results.Ok(files);
});

// Student: Get their team's tasks
app.MapGet("/api/student/tasks", async (Guid studentId, AppDbContext db) =>
{
    var student = await db.Students.Include(s => s.Team).FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student?.Team == null)
        return Results.BadRequest(new { Success = false, Message = "Student not found or not in a team." });

    var tasks = await db.Tasks
        .Where(t => t.TeamId == student.TeamId)
        .Include(t => t.Milestones)
        .OrderByDescending(t => t.CreatedAt)
        .ToListAsync();

    foreach (var taskItem in tasks)
    {
        taskItem.Milestones = taskItem.Milestones.OrderBy(m => m.Order).ToList();
    }

    return Results.Ok(tasks);
});

// Student: Complete a task
app.MapPost("/api/student/complete-task/{taskId}", async (Guid taskId, AppDbContext db) =>
{
    var task = await db.Tasks.FindAsync(taskId);
    if (task == null)
        return Results.NotFound(new { Success = false, Message = "Task not found." });

    task.IsCompleted = true;
    task.CompletedAt = DateTime.UtcNow;
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true });
});

// Get team files for supervisor (sorted by late first)
app.MapGet("/api/supervisor/team-files/{teamId}", async (Guid teamId, Guid supervisorId, AppDbContext db) =>
{
    try
    {
        var team = await db.Teams.FirstOrDefaultAsync(t => t.TeamId == teamId);
        if (team == null)
            return Results.NotFound(new { Success = false, Message = "Team not found." });

        if (team.SupervisorId != supervisorId)
            return Results.Unauthorized();

        var files = await db.FileSubmissions
            .Where(f => f.TeamId == teamId)
            .Include(f => f.Comments)
                .ThenInclude(c => c.Supervisor)
            .OrderByDescending(f => f.IsLate)
            .ThenByDescending(f => f.SubmittedAt)
            .Select(f => new {
                f.FileSubmissionId,
                f.FileName,
                f.FilePath,
                f.SubmittedAt,
                f.IsLate,
                f.Description,
                CommentsCount = f.Comments.Count,
                Comments = f.Comments.Select(c => new {
                    c.FileCommentId,
                    c.Content,
                    c.CreatedAt,
                    SupervisorName = c.Supervisor != null ? c.Supervisor.SupervisorName : "Unknown"
                }).ToList()
            })
            .ToListAsync();

        return Results.Ok(files);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] /api/supervisor/team-files: {ex.Message}");
        return Results.Problem("Internal Server Error: " + ex.Message);
    }
});

// Get all messages related to a supervisor (sent by them or received by them/their teams)
app.MapGet("/api/supervisor/my-messages", async (Guid supervisorId, AppDbContext db) =>
{
    var supervisor = await db.Supervisors.FindAsync(supervisorId);
    if (supervisor == null) return Results.Unauthorized();

    // Find all teams supervised by this person
    var teamIds = await db.Teams
        .Where(t => t.SupervisorId == supervisorId)
        .Select(t => t.TeamId)
        .ToListAsync();

    var msgs = await db.Messages
        .Where(m => m.SenderId == supervisorId || 
                    m.ReceiverSupervisorId == supervisorId || 
                    (m.TeamId != null && teamIds.Contains(m.TeamId.Value)))
        .OrderByDescending(m => m.SentAt)
        .ToListAsync();

    var result = new List<object>();
    foreach (var m in msgs)
    {
        string senderName = "Unknown";
        if (m.SenderRole == Role.Supervisor)
        {
            var s = await db.Supervisors.FindAsync(m.SenderId);
            if (s != null) senderName = s.SupervisorName;
        }
        else if (m.SenderRole == Role.Student)
        {
            var s = await db.Students.FindAsync(m.SenderId);
            if (s != null) senderName = s.StudentName;
        }
        else if (m.SenderRole == Role.Coordinator)
        {
            var c = await db.Coordinators.FindAsync(m.SenderId);
            if (c != null) senderName = c.CoordinatorName;
        }

        // Try to get team name if applicable
        string teamName = "Direct/Unknown";
        if (m.TeamId != null)
        {
            var team = await db.Teams.FindAsync(m.TeamId);
            if (team != null) teamName = team.TeamName;
        }

        result.Add(new {
            m.MessageId,
            m.SenderId,
            m.SenderRole,
            SenderName = senderName,
            m.TeamId,
            TeamName = teamName,
            m.ReceiverSupervisorId,
            m.Content,
            m.SentAt,
            CreatedAt = m.SentAt // frontend expects createdAt
        });
    }

    return Results.Ok(result);
});

// Add comment to file
app.MapPost("/api/supervisor/file-comment", async (AddFileCommentRequest request, Guid supervisorId, AppDbContext db) =>
{
    try
    {
        var submission = await db.FileSubmissions.Include(f => f.Team).FirstOrDefaultAsync(f => f.FileSubmissionId == request.FileSubmissionId);
        if (submission == null)
            return Results.NotFound(new { Success = false, Message = "File not found." });

        if (submission.Team == null)
             return Results.BadRequest(new { Success = false, Message = "Team reference missing on this file." });

        if (submission.Team.SupervisorId != supervisorId)
            return Results.Unauthorized();

        var comment = new FileComment
        {
            FileCommentId = Guid.NewGuid(),
            FileSubmissionId = request.FileSubmissionId,
            SupervisorId = supervisorId,
            Content = request.Content,
            CreatedAt = DateTime.UtcNow
        };

        db.FileComments.Add(comment);
        await db.SaveChangesAsync();

        return Results.Ok(new { Success = true, FileCommentId = comment.FileCommentId });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR] /api/supervisor/file-comment: {ex.Message}");
        return Results.Problem("Internal Server Error: " + ex.Message);
    }
});

// --- Committee / اللجنة Feature ---

// Supervisor: Set Availability
app.MapPost("/api/supervisor/availability", async (SetAvailabilityRequest request, Guid supervisorId, AppDbContext db) =>
{
    if (request.Date.Date < DateTime.Today)
    {
        return Results.BadRequest(new { Success = false, Message = "لا يمكن حجز موعد في تاريخ قديم." });
    }

    var supervisor = await db.Supervisors.FindAsync(supervisorId);
    if (supervisor == null) return Results.Unauthorized();

    var availability = new CommitteeAvailability
    {
        CommitteeAvailabilityId = Guid.NewGuid(),
        SupervisorId = supervisorId,
        AvailableDate = request.Date,
        TimeSlot = request.TimeSlot
    };

    db.CommitteeAvailabilities.Add(availability);
    await db.SaveChangesAsync();

    return Results.Ok(new { Success = true });
});

// Supervisor: Get own availabilities
app.MapGet("/api/supervisor/availability", async (Guid supervisorId, AppDbContext db) =>
{
    var list = await db.CommitteeAvailabilities
        .Where(a => a.SupervisorId == supervisorId)
        .OrderByDescending(a => a.AvailableDate)
        .ToListAsync();
    return Results.Ok(list);
});

// Coordinator: Get all supervisors' availability
app.MapGet("/api/coordinator/all-availabilities", async (AppDbContext db) =>
{
    var list = await db.CommitteeAvailabilities
        .Include(a => a.Supervisor)
        .OrderByDescending(a => a.AvailableDate)
        .Select(a => new {
            a.CommitteeAvailabilityId,
            a.SupervisorId,
            a.Supervisor.SupervisorName,
            a.AvailableDate,
            a.TimeSlot
        })
        .ToListAsync();
    return Results.Ok(list);
});

// Coordinator: Assign committee to a project
app.MapPost("/api/coordinator/assign-committee", async (AssignCommitteeRequest request, AppDbContext db) =>
{
    var team = await db.Teams
        .Include(t => t.Project)
            .ThenInclude(p => p.CommitteeMembers)
        .Include(t => t.Members)
        .FirstOrDefaultAsync(t => t.TeamId == request.TeamId);
    
    if (team == null) return Results.NotFound("Team not found");
    if (team.Project == null) return Results.BadRequest("Team has no project");

    var project = team.Project;

    // --- Validation: Past Date ---
    if (request.PresentationDate < DateTime.Now)
    {
        return Results.BadRequest(new { Success = false, Message = "Presentation date cannot be in the past." });
    }

    // --- Validation: Supervisor Availability ---
    var requiredSupervisors = new List<Guid> { request.MainSupervisorId, request.Sup2Id, request.Sup3Id };
    
    // We assume the time slots are like "09:00 - 10:00", etc. 
    // Since we receive a DateTime, we need to match both Date and TimeSlot.
    // For simplicity, let's assume if the date matches and they have ANY slot that covers that time.
    // However, the current system uses string TimeSlots.
    // Let's try to find if they have an availability for that DATE.
    // If the user wants stricter "TimeSlot" matching, we'd need to parse the time.
    // For now, let's check if the supervisor has an availability record on that date.
    
    var presentationDateOnly = request.PresentationDate.Date;
    
    foreach (var supId in requiredSupervisors)
    {
        var hasAvailability = await db.CommitteeAvailabilities
            .AnyAsync(a => a.SupervisorId == supId && a.AvailableDate.Date == presentationDateOnly);
        
        if (!hasAvailability)
        {
            var sup = await db.Supervisors.FindAsync(supId);
            return Results.BadRequest(new { Success = false, Message = $"المشرف {sup?.SupervisorName} غير متاح في هذا التاريخ." });
        }
    }

    project.PresentationDate = request.PresentationDate;

    // Remove existing if any
    db.CommitteeMembers.RemoveRange(project.CommitteeMembers);

    var members = new List<CommitteeMember>
    {
        new CommitteeMember { CommitteeMemberId = Guid.NewGuid(), ProjectId = project.ProjectId, SupervisorId = request.MainSupervisorId, IsMainSupervisor = true },
        new CommitteeMember { CommitteeMemberId = Guid.NewGuid(), ProjectId = project.ProjectId, SupervisorId = request.Sup2Id, IsMainSupervisor = false },
        new CommitteeMember { CommitteeMemberId = Guid.NewGuid(), ProjectId = project.ProjectId, SupervisorId = request.Sup3Id, IsMainSupervisor = false }
    };

    db.CommitteeMembers.AddRange(members);
    await db.SaveChangesAsync();

    // ===== NEW: Send notifications to all team members =====
    var presentationDateFormatted = project.PresentationDate?.ToString("yyyy-MM-dd HH:mm") ?? "N/A";
    var notificationMessage = $"تم تعيين لجنة مناقشة لمشروعك! المناقشة في: {presentationDateFormatted}";
    
    if (team.Members != null && team.Members.Count > 0)
    {
        var notifications = new List<Notification>();
        foreach (var member in team.Members)
        {
            notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = member.StudentId,
                Message = notificationMessage,
                Type = "CommitteeAssigned",
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
                RelatedId = project.ProjectId.ToString()
            });
        }
        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { Success = true, Message = "Committee assigned and notifications sent to team" });
});

// Coordinator: Get assigned committees (for viewing past assignments)
app.MapGet("/api/coordinator/assigned-committees", async (AppDbContext db) =>
{
    var assignments = await db.CommitteeMembers
        .Include(cm => cm.Project)
            .ThenInclude(p => p.Team)
        .Include(cm => cm.Supervisor)
        .GroupBy(cm => cm.ProjectId)
        .Select(g => new {
            ProjectId = g.Key,
            ProjectTitle = g.First().Project.Title,
            TeamName = g.First().Project.Team.TeamName,
            PresentationDate = g.First().Project.PresentationDate,
            MainSupervisor = g.FirstOrDefault(cm => cm.IsMainSupervisor).Supervisor.SupervisorName,
            MainSupervisorGrade = g.FirstOrDefault(cm => cm.IsMainSupervisor).Grade,
            CommitteeMembers = g.Where(cm => !cm.IsMainSupervisor)
                .Select(cm => new { cm.Supervisor.SupervisorName, cm.Grade })
                .ToList()
        })
        .OrderByDescending(a => a.PresentationDate)
        .ToListAsync();

    return Results.Ok(assignments);
});

// Supervisor: Get upcoming presentations (projects assigned to grade)
app.MapGet("/api/supervisor/upcoming-presentations", async (Guid supervisorId, AppDbContext db) =>
{
    var presentations = await db.CommitteeMembers
        .Include(cm => cm.Project)
            .ThenInclude(p => p.Team)
        .Where(cm => cm.SupervisorId == supervisorId && cm.Project.PresentationDate != null)
        .Select(cm => new {
            cm.ProjectId,
            ProjectTitle = cm.Project.Title,
            TeamName = cm.Project.Team.TeamName,
            PresentationDate = cm.Project.PresentationDate,
            cm.IsMainSupervisor,
            cm.Grade,
            RoleDisplay = cm.IsMainSupervisor ? "مشرف الفريق" : "عضو لجنة",
            MaxGrade = cm.IsMainSupervisor ? 50 : 25
        })
        .OrderBy(p => p.PresentationDate)
        .ToListAsync();

    return Results.Ok(presentations);
});

// Supervisor: Get projects they are assigned to grade (after presentation date)
app.MapGet("/api/supervisor/pending-grading", async (Guid supervisorId, AppDbContext db) =>
{
    var now = DateTime.UtcNow;
    var assignments = await db.CommitteeMembers
        .Include(cm => cm.Project)
            .ThenInclude(p => p.Team)
        .Where(cm => cm.SupervisorId == supervisorId && cm.Project.PresentationDate != null && cm.Project.PresentationDate.Value.AddMinutes(30) <= now && cm.Grade == null)
        .Select(cm => new {
            cm.ProjectId,
            ProjectTitle = cm.Project.Title,
            TeamName = cm.Project.Team.TeamName,
            PresentationDate = cm.Project.PresentationDate,
            cm.IsMainSupervisor,
            MaxGrade = cm.IsMainSupervisor ? 50 : 25
        })
        .ToListAsync();

    return Results.Ok(assignments);
});

// Supervisor: Submit Grade
app.MapPost("/api/supervisor/submit-grade", async (SubmitGradeRequest request, Guid supervisorId, AppDbContext db) =>
{
    var cm = await db.CommitteeMembers.Include(m => m.Project).FirstOrDefaultAsync(m => m.ProjectId == request.ProjectId && m.SupervisorId == supervisorId);
    if (cm == null) return Results.NotFound("Assignment not found");

    if (cm.Project.PresentationDate == null || cm.Project.PresentationDate.Value.AddMinutes(30) > DateTime.UtcNow)
    {
        return Results.BadRequest("Grading is only allowed 30 minutes after the presentation start time.");
    }

    var maxGrade = cm.IsMainSupervisor ? 50 : 25;
    if (request.Grade < 0 || request.Grade > maxGrade)
        return Results.BadRequest($"Grade must be between 0 and {maxGrade}");

    cm.Grade = request.Grade;
    cm.GradedAt = DateTime.UtcNow;

    await db.SaveChangesAsync();

    // Notify student about the grade
    var project = await db.Projects.Include(p => p.Team).ThenInclude(t => t.Members).FirstOrDefaultAsync(p => p.ProjectId == request.ProjectId);
    if (project != null)
    {
        foreach (var member in project.Team.Members)
        {
            db.Notifications.Add(new Notification
            {
                NotificationId = Guid.NewGuid(),
                UserId = member.StudentId,
                Message = $"A grade of {request.Grade}/{maxGrade} has been submitted for your project by a committee member.",
                Type = "GradeSubmitted",
                RelatedId = project.ProjectId.ToString(),
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            });
        }
        await db.SaveChangesAsync();
    }

    return Results.Ok(new { Success = true });
});

// Get all files for supervisor to view across all teams
app.MapGet("/api/supervisor/all-team-files", async (Guid supervisorId, AppDbContext db) =>
{
    var files = await db.FileSubmissions
        .Include(f => f.Team)
        .Include(f => f.Comments)
        .Where(f => f.Team.SupervisorId == supervisorId)
        .OrderByDescending(f => f.IsLate)
        .ThenByDescending(f => f.SubmittedAt)
        .Select(f => new {
            f.FileSubmissionId,
            f.FileName,
            f.FilePath,
            f.SubmittedAt,
            f.IsLate,
            TeamName = f.Team.TeamName,
            TeamId = f.Team.TeamId,
            CommentsCount = f.Comments.Count
        })
        .ToListAsync();

    return Results.Ok(files);
});

// Get comments on a file for viewing by team
app.MapGet("/api/student/file-comments/{fileSubmissionId}", async (Guid fileSubmissionId, Guid studentId, AppDbContext db) =>
{
    var submission = await db.FileSubmissions
        .Include(f => f.Team)
        .ThenInclude(t => t.Members)
        .Include(f => f.Comments)
        .ThenInclude(c => c.Supervisor)
        .FirstOrDefaultAsync(f => f.FileSubmissionId == fileSubmissionId);

    if (submission == null)
        return Results.NotFound(new { Success = false, Message = "File not found." });

    var student = await db.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);
    if (student?.TeamId != submission.TeamId)
        return Results.Unauthorized();

    var comments = submission.Comments.Select(c => new {
        c.FileCommentId,
        c.Content,
        c.CreatedAt,
        SupervisorName = c.Supervisor.SupervisorName
    }).ToList();

    return Results.Ok(comments);
});

// Get notifications for a user
app.MapGet("/api/notifications/{userId}", async (Guid userId, AppDbContext db) =>
{
    var notifications = await db.Notifications
        .Where(n => n.UserId == userId)
        .OrderByDescending(n => n.CreatedAt)
        .ToListAsync();
    return Results.Ok(notifications);
});

// Mark notification as read
app.MapPost("/api/notifications/{notificationId}/read", async (Guid notificationId, AppDbContext db) =>
{
    var notification = await db.Notifications.FindAsync(notificationId);
    if (notification == null) return Results.NotFound();

    notification.IsRead = true;
    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

// Mark all notifications as read for a user
app.MapPost("/api/notifications/read-all/{userId}", async (Guid userId, AppDbContext db) =>
{
    var notifications = await db.Notifications.Where(n => n.UserId == userId && !n.IsRead).ToListAsync();
    foreach (var n in notifications)
    {
        n.IsRead = true;
    }
    await db.SaveChangesAsync();
    return Results.Ok(new { Success = true });
});

app.Run();
