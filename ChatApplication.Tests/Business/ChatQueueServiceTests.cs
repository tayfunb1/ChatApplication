using Xunit;
using ChatApplication.Business.Concrete;
using ChatApplication.Business.Models.DataAccess.Entities;
using ChatApplication.Business.Models.DataAccess.Entities.Enums;
using FluentAssertions;

namespace ChatApplication.Tests.Business;

public class ChatQueueServiceTests
{
    private readonly ChatQueueService _service = new();

    [Fact]
    public async Task DequeueChatSessionWithCondition_Should_Return_Null_If_Condition_Does_Not_Match()
    {
        // Arrange
        var team2 = new Team
        {
            Id = 2,
            Name = "Team 2",
            Agents = new List<Agent>
            {
                new Agent { Name = "Agent 3", AgentType = AgentType.Junior },
                new Agent { Name = "Agent 4", AgentType = AgentType.TeamLead }
            }
        };

        var session1 = new ChatSession { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow, AssignedTeam = team2 };
        var session2 = new ChatSession
            { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow.AddMinutes(-1), AssignedTeam = team2 };

        await _service.QueueChatSession(session1, team2);
        await _service.QueueChatSession(session2, team2);

        // Act
        var result = await _service.DequeueChatSessionWithCondition(x => x.Id == Guid.NewGuid());

        // Assert
        result.Should().BeNull(); // The session with this ID doesn't exist in the queue
    }
}