namespace SuperSee;

public class StudentCapability
{
    private string name;
    public string Name
    {
        get => name;
        set => name = value;
    }

    private Guid studentId;
    public Guid StudentId
    {
        get => studentId;
        set => studentId = value;
    }

    private Student student;
    public Student Student
    {
        get => student;
        set => student = value;
    }
}