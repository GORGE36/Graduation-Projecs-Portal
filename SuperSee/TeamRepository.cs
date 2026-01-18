using System;
using System.Collections.Generic;
using System.Linq;

namespace SuperSee;

public class TeamRepository
{
    private List<Team> _teams = new List<Team>();

    public void AddTeam(Team team)
    {
        _teams.Add(team);
    }

    public List<Team> GetAllTeams()
    {
        return _teams;
    }

    public Team GetTeamById(Guid teamId)
    {
        return _teams.FirstOrDefault(t => t.TeamId == teamId);
    }
}