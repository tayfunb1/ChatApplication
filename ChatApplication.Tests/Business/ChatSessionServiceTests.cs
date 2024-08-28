using System.Collections.Concurrent;
using ChatApplication.Business.Abstract;
using ChatApplication.Business.Concrete;
using ChatApplication.Business.Models.Common;
using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Entities;
using ChatApplication.DataAccess.Entities.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace ChatApplication.Tests.Business;

public class ChatSessionServiceTests
{
    private readonly Mock<IChatQueueManager> _mockChatQueueService;
    private readonly Mock<ITeamService> _mockTeamService;
    private readonly ConcurrentDictionary<Guid, ChatSession> _assignedChatSessions;
    private readonly ChatSessionService _service;

    public ChatSessionServiceTests()
    {
        _mockChatQueueService = new Mock<IChatQueueManager>();
        _mockTeamService = new Mock<ITeamService>();
        _assignedChatSessions = new ConcurrentDictionary<Guid, ChatSession>();
        _service = new ChatSessionService(_mockChatQueueService.Object, _mockTeamService.Object, _assignedChatSessions);
    }
    
    [Fact]
    public async Task StartChatSessionAsync_Should_Return_Success_Response_When_Team_Is_Available_And_Chat_Is_Queued()
    {
        // Arrange
        var shiftId = (int)Shift.Morning;
        var chatSession = new ChatSession { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var activeTeam = new Team
        {
            Id = 1,
            Name = "Team A",
            Shift = Shift.Morning,
            Agents = new List<Agent> { new Agent { Name = "Agent A", AgentType = AgentType.Junior } }
        };

        _mockTeamService.Setup(ts => ts.GetActiveTeam(shiftId)).Returns(activeTeam);
        _mockChatQueueService.Setup(cq => cq.QueueChatSession(It.IsAny<ChatSession>(), It.IsAny<Team>()))
            .ReturnsAsync(true);

        // Act
        var result = await _service.StartChatSessionAsync(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ResponseCode.Should().Be(ResponseCodes.Ok);
        result.Message.Should().Be("Chat session is queued successfully");
        result.Data.Id.Should().NotBe(Guid.Empty);
    }
    
    [Fact]
    public async Task StartChatSessionAsync_Should_Return_Error_Response_When_No_Team_Is_Available()
    {
        // Arrange
        var shiftId = (int)Shift.Morning;

        _mockTeamService.Setup(ts => ts.GetActiveTeam(shiftId)).Returns((Team)null);

        // Act
        var result = await _service.StartChatSessionAsync(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ResponseCode.Should().Be(ResponseCodes.InternalServerError);
        result.Message.Should().Be("An error occured during team selection");
    }
    
    [Fact]
    public async Task StartChatSessionAsync_Should_Return_Error_Response_When_Chat_Is_Not_Queued()
    {
        // Arrange
        var shiftId = (int)Shift.Morning;
        var chatSession = new ChatSession { Id = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        var activeTeam = new Team
        {
            Id = 1,
            Name = "Team A",
            Shift = Shift.Morning,
            Agents = new List<Agent> { new Agent { Name = "Agent A", AgentType = AgentType.Junior } }
        };

        _mockTeamService.Setup(ts => ts.GetActiveTeam(shiftId)).Returns(activeTeam);
        _mockChatQueueService.Setup(cq => cq.QueueChatSession(It.IsAny<ChatSession>(), It.IsAny<Team>()))
            .ReturnsAsync(false);

        // Act
        var result = await _service.StartChatSessionAsync(shiftId);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.ResponseCode.Should().Be(ResponseCodes.ServiceUnavailable);
        result.Message.Should().Be("No agents available at the moment, please try again later");
    }
    
    [Fact]
    public async Task DisplayAssignedChatSessions_Should_Return_Chat_Sessions()
    {
        // Arrange
        var chatSessionId = Guid.NewGuid();
        var chatSession = new ChatSession
        {
            Id = chatSessionId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            PollCount = 1,
            IsToBePolled = true,
            AssignedTeam = new Team
            {
                Name = "Team A",
                Agents = new List<Agent>
                {
                    new Agent { Name = "Agent A", AgentType = AgentType.Junior }
                }
            }
        };
        _assignedChatSessions.TryAdd(chatSessionId, chatSession);

        // Act
        var result = await _service.DisplayAssignedChatSessions();

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ResponseCode.Should().Be(ResponseCodes.Ok);
        result.Message.Should().Be("Assigned chat session data acquired successfully");
        result.DataList.Should().HaveCount(1);
        result.DataList.First().Id.Should().Be(chatSessionId);
        result.DataList.First().CreatedAt.Should().Be(chatSession.CreatedAt);
    }
}