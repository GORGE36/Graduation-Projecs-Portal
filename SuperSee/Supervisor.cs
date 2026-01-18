namespace SuperSee;

public class Supervisor
{
    private Guid supervisorId;
    public Guid SupervisorId
    {
        get => supervisorId;
        set => supervisorId = value;
    }

    private string supervisorName;
    public string SupervisorName
    {
        get => supervisorName;
        set => supervisorName = value;
    }

    private string supervisorEmail;
    public string SupervisorEmail
    {
        get => supervisorEmail;
        set => supervisorEmail = value;
    }

    private string supervisorPasswordHash;
    public string SupervisorPasswordHash
    {
        get => supervisorPasswordHash;
        set => supervisorPasswordHash = value;
    }

    private List<Team> teams;
    public List<Team> Teams
    {
        get => teams;
        set => teams = value;
    }

    private Guid? coordinatorId;
    public Guid? CoordinatorId
    {
        get => coordinatorId;
        set => coordinatorId = value;
    }

    private Coordinator coordinator;
    public Coordinator Coordinator
    {
        get => coordinator;
        set => coordinator = value;
    }

    public Supervisor() { }

    public Supervisor(string SupervisorName, string SupervisorEmail, string SupervisorPasswordHash, Guid CoordinatorId)
    {
        this.SupervisorId = Guid.NewGuid();
        this.CoordinatorId = CoordinatorId;
        this.SupervisorName = SupervisorName;
        this.SupervisorEmail = SupervisorEmail;
        this.SupervisorPasswordHash = SupervisorPasswordHash;
        this.Teams = new List<Team>();
    }
}