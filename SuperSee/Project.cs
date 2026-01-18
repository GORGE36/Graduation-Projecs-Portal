namespace SuperSee;

public class Project
{
    private Guid projectId;
    public Guid ProjectId
    {
        get => projectId;
        set => projectId = value;
    }

    private string title;
    public string Title
    {
        get => title;
        set => title = value;
    }

    private string description;
    public string Description
    {
        get => description;
        set => description = value;
    }

    private DateTime deadline;
    public DateTime Deadline
    {
        get => deadline;
        set => deadline = value;
    }

    private Status status;
    public Status Status
    {
        get => status;
        set => status = value;
    }

    private Guid teamId;
    public Guid TeamId
    {
        get => teamId;
        set => teamId = value;
    }

    private Team team;
    public Team Team
    {
        get => team;
        set => team = value;
    }

    public Project() { }

    public Project(string Title, string Description, DateTime Deadline, Guid TeamId)
    {
        this.ProjectId = Guid.NewGuid();
        this.Title = Title;
        this.Description = Description;
        this.Deadline = Deadline;
        this.TeamId = TeamId;
        this.Status = Status.NotStarted;
    }
}