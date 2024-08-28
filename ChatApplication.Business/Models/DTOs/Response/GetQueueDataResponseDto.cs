namespace ChatApplication.Business.Models.DTOs.Response;

public class GetQueueDataResponseDto : ChatSessionResponseDto
{
    public bool IsActive { get; set; }
    public int PollCount { get; set; }
    public bool IsToBePolled { get; set; } = true;
    public ChatSessionTeamResponse AssignedTeam { get; set; }
}

public class ChatSessionTeamResponse
{
    public string Name { get; set; }
    public string AssignedAgent { get; set; }
}