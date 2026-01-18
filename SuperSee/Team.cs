using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SuperSee;

public class Team
{
    private Guid teamId;
    public Guid TeamId
    {
        get => teamId;
        set => teamId = value;
    }

    private string teamName;
    public string TeamName
    {
        get => teamName;
        set => teamName = value;
    }

    private Guid supervisorId;
    public Guid SupervisorId
    {
        get => supervisorId;
        set => supervisorId = value;
    }

    private Supervisor supervisor;
    public Supervisor Supervisor
    {
        get => supervisor;
        set => supervisor = value;
    }

    private Coordinator coordinator;
    public Coordinator Coordinator
    {
        get => coordinator;
        set => coordinator = value;
    }

    private Guid? coordinatorId;
    public Guid? CoordinatorId
    {
        get => coordinatorId;
        set => coordinatorId = value;
    }

    private Guid? teamLeaderId;
    public Guid? TeamLeaderId
    {
        get => teamLeaderId;
        set => teamLeaderId = value;
    }

    private Student teamLeader;
    public Student TeamLeader
    {
        get => teamLeader;
        set => teamLeader = value;
    }

    private AssignmentStatus assignmentStatus;
    public AssignmentStatus AssignmentStatus
    {
        get => assignmentStatus;
        set => assignmentStatus = value;
    }

    private Project project;
    public Project Project
    {
        get => project;
        set => project = value;
    }

    private List<Student> members;
    public List<Student> Members
    {
        get => members;
        set => members = value;
    }

    public Team() { }

    public Team(string TeamName, Guid SupervisorId, Guid? CoordinatorId = null, Guid? TeamLeaderId = null)
    {
        this.TeamId = Guid.NewGuid();
        this.TeamName = TeamName;
        this.SupervisorId = SupervisorId;
        this.CoordinatorId = CoordinatorId;
        this.TeamLeaderId = TeamLeaderId;
        this.AssignmentStatus = AssignmentStatus.Pending;
        this.Members = new List<Student>();
    }
}