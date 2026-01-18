namespace SuperSee;

public class Student
{
    private Guid studentId;
    public Guid StudentId
    {
        get => studentId;
        set => studentId = value;
    }

    private string studentName;
    public string StudentName
    {
        get => studentName;
        set => studentName = value;
    }
    

    private string studentEmail;
    public string StudentEmail
    {
        get => studentEmail;
        set => studentEmail = value;
    }

    private string studentPasswordHash;
    public string StudentPasswordHash
    {
        get => studentPasswordHash;
        set => studentPasswordHash = value;
    }

    private Role role;
    public Role Role
    {
        get => role;
        set => role = value;
    }

    private Guid? teamId;
    public Guid? TeamId
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

    private List<StudentCapability> capabilities;
    public List<StudentCapability> Capabilities
    {
        get => capabilities;
        set => capabilities = value;
    }

    public Student() { }

    public Student(string StudentName, string StudentEmail, string StudentPasswordHash)
    {
        this.StudentId = Guid.NewGuid();
        this.StudentName = StudentName;
        this.StudentEmail = StudentEmail;
        this.StudentPasswordHash = StudentPasswordHash;
        this.Capabilities = new List<StudentCapability>();
    }
}