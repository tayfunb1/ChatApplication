using ChatApplication.Business.Concrete;
using ChatApplication.DataAccess.Entities;
using ChatApplication.DataAccess.Entities.Enums;
using FluentAssertions;
using Xunit;

namespace ChatApplication.Tests.Business;

public class TeamServiceTests
{
    private static (List<Team> Teams, OverflowTeam OverflowTeam) SetInitialTeams()
    {
        var teams = new List<Team>
        {
            new Team
            {
                Id = 1,
                Name = "Team A",
                Shift = Shift.Morning,
                Agents = new List<Agent>
                {
                    new Agent { Name = "Team Lead A", AgentType = AgentType.TeamLead },
                    new Agent { Name = "Mid-Level A1", AgentType = AgentType.MidLevel },
                    new Agent { Name = "Mid-Level A2", AgentType = AgentType.MidLevel },
                    new Agent { Name = "Junior A", AgentType = AgentType.Junior }
                }
            },
            new Team
            {
                Id = 2,
                Name = "Team B",
                Shift = Shift.Afternoon,
                Agents = new List<Agent>
                {
                    new Agent { Name = "Senior B", AgentType = AgentType.Senior },
                    new Agent { Name = "Mid-Level B", AgentType = AgentType.MidLevel },
                    new Agent { Name = "Junior B1", AgentType = AgentType.Junior },
                    new Agent { Name = "Junior B2", AgentType = AgentType.Junior }
                }
            },
            new Team
            {
                Id = 3,
                Name = "Team C",
                Shift = Shift.Night,
                Agents = new List<Agent>
                {
                    new Agent { Name = "Mid-Level C1", AgentType = AgentType.MidLevel },
                    new Agent { Name = "Mid-Level C2", AgentType = AgentType.MidLevel }
                }
            }
        };

        var overflowTeam = new OverflowTeam
        {
            Id = 4,
            Name = "Team Overflow",
            Agents = new List<Agent>
            {
                new Agent { Name = "Overflow1", AgentType = AgentType.Overflow },
                new Agent { Name = "Overflow2", AgentType = AgentType.Overflow },
                new Agent { Name = "Overflow3", AgentType = AgentType.Overflow },
                new Agent { Name = "Overflow4", AgentType = AgentType.Overflow },
                new Agent { Name = "Overflow5", AgentType = AgentType.Overflow },
                new Agent { Name = "Overflow6", AgentType = AgentType.Overflow }
            }
        };

        return (teams, overflowTeam);
    }

    [Fact]
    public void GetActiveTeam_Should_Return_Team_Without_Overflow_Agents_Outside_Office_Hours()
    {
        // Arrange
        TimeSpan officeHoursStart = new(9, 0, 0);
        TimeSpan officeHoursEnd = new(17, 0, 0);
        
        var now = DateTime.UtcNow.TimeOfDay;
        var isOfficeHours = now >= officeHoursStart && now <= officeHoursEnd;
        
        var service = new TeamService(GetTeams(), GetOverflowTeam());

        // Act
        var result = service.GetActiveTeam((int)Shift.Morning);

        // Assert
        result.Should().NotBeNull();
        if (isOfficeHours)
        {
            result.Agents.Should().HaveCount(10);
            result.Agents.Should().Contain(a => a.Name == "Overflow1");
        }
        
        else
        {
            result.Agents.Should().HaveCount(4); // Only agents from team, no overflow agents
            result.Agents.Should().NotContain(a => a.Name == "Overflow1");
        }
    }

    [Fact]
    public void GetActiveTeam_Should_Return_Null_If_No_Team_Is_Active()
    {
        // Arrange
        var teams = new List<Team>
        {
            new Team
            {
                Id = 1,
                Name = "Inactive Team",
                Shift = Shift.Night, // The shift we're testing
                Agents = new List<Agent>() // No available agents
            }
        };

        var overflowTeam = GetOverflowTeam();
        var service = new TeamService(teams, overflowTeam);

        // Act
        var result = service.GetActiveTeam((int)Shift.Night); // Checking Night shift

        // Assert
        result.Should().BeNull(); // Expecting null because no active team in the Night shift
    }

    [Fact]
    public void GetActiveTeam_Should_Return_Null_If_No_Agents_Are_Available()
    {
        // Arrange
        var teams = new List<Team>
        {
            new Team
            {
                Id = 1,
                Name = "Team D",
                Shift = Shift.Morning,
                Agents = new List<Agent>() // No agents available
            }
        };

        var service = new TeamService(teams, GetOverflowTeam());

        // Act
        var result = service.GetActiveTeam((int)Shift.Morning);

        // Assert
        result.Should().BeNull();
    }

    private static IEnumerable<Team> GetTeams()
    {
        return SetInitialTeams().Teams;
    }

    private static OverflowTeam GetOverflowTeam()
    {
        return SetInitialTeams().OverflowTeam;
    }
}