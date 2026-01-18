namespace SuperSee;

public class UserContext
{
    private Guid userId;
    public Guid UserId
    {
        get => userId;
        set => userId = value;
    }

    private Role role;
    public Role Role
    {
        get => role;
        set => role = value;
    }
    
    public UserContext(Guid userId, Role role)
    {
        UserId = userId;
        Role = role;
    }
}