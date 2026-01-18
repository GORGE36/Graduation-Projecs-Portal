namespace SuperSee;

public class Coordinator
{
    private Guid coordinatorId;
    public Guid CoordinatorId
    {
        get => coordinatorId;
        set => coordinatorId = value;
    }

    private string coordinatorName;
    public string CoordinatorName
    {
        get => coordinatorName;
        set => coordinatorName = value;
    }

    private string coordinatorEmail;
    public string CoordinatorEmail
    {
        get => coordinatorEmail;
        set => coordinatorEmail = value;
    }

    private string coordinatorPasswordHash;
    public string CoordinatorPasswordHash
    {
        get => coordinatorPasswordHash;
        set => coordinatorPasswordHash = value;
    }

    private List<Supervisor> supervisors;
    public List<Supervisor> Supervisors
    {
        get => supervisors;
        set => supervisors = value;
    }

    public Coordinator() { }

    public Coordinator(string CoordinatorName, string CoordinatorEmail, string CoordinatorPasswordHash)
    {
        this.CoordinatorId = Guid.NewGuid();
        this.CoordinatorName = CoordinatorName;
        this.CoordinatorEmail = CoordinatorEmail;
        this.CoordinatorPasswordHash = CoordinatorPasswordHash;
        this.Supervisors = new List<Supervisor>();
    }
}