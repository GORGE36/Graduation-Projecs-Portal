using Xunit;
using SuperSee;
using System;

namespace SuperSee.Tests;

public class TeamRepositoryTests
{
    [Fact]
    public void AddTeam_AddsToCollection()
    {
        var repo = new TeamRepository();
        var team = new Team("Test Team", Guid.NewGuid());
        
        repo.AddTeam(team);
        
        Assert.Single(repo.GetAllTeams());
        Assert.Equal(team, repo.GetTeamById(team.TeamId));
    }
}