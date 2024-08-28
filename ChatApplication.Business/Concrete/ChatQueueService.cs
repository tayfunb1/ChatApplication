using ChatApplication.Business.Abstract;
using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DTOs.Response;
using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Entities;

namespace ChatApplication.Business.Concrete;

public class ChatQueueService(IChatQueueManager queueManager) : IChatQueueService
{
    private const int PollThreshold = 3;
    
    public async Task<BaseApiListResponse<GetQueueDataResponseDto>> DisplayQueueData()
    {
        var result = await queueManager.DisplayQueueData();
        return new BaseApiListResponse<GetQueueDataResponseDto>
        {
            Message = "Queue data acquired successfully",
            DataList = result.Select(x => new GetQueueDataResponseDto
            {
                Id = x.Id,
                CreatedAt = x.CreatedAt,
                PollCount = x.PollCount,
                IsActive = x.IsActive,
                IsToBePolled = x.IsToBePolled,
                AssignedTeam = new ChatSessionTeamResponse
                {
                    Name = x.AssignedTeam.Name,
                    AssignedAgent = x.AssignedTeam.Agents.FirstOrDefault(y => y.ChatSessions.Contains(x))?.Name
                }
            }).ToList(),
            Success = true,
            ResponseCode = ResponseCodes.Ok
        };
    }

    public async Task<BaseApiResponse<ChatSession>> PollChatSession(Guid chatSessionId)
    {
        var session = await queueManager.PollChatSession(chatSessionId);
        if (session is not null)
        {
            if (session.CreatedAt.AddSeconds(3) < DateTime.UtcNow)
            {
                session.IsToBePolled = false;
                return new BaseApiResponse<ChatSession>
                {
                    Data = session,
                    Success = true,
                    ResponseCode = ResponseCodes.Ok,
                    Message = 
                        $"Poll unsuccessful, Session Created Time:{session.CreatedAt.TimeOfDay}, " +
                        $"Poll Time: {DateTime.UtcNow.TimeOfDay}"
                };
            }
            
            if (session.PollCount < PollThreshold)
            {
                session.PollCount++;
                if (session.PollCount == PollThreshold)
                {
                    session.IsActive = true;
                    session.IsToBePolled = false;
                }
            }

            return new BaseApiResponse<ChatSession>
            {
                Data = session,
                Success = true,
                ResponseCode = ResponseCodes.Ok,
                Message = $"Chat session with Id: {session.Id} has been polled"
            };
        }
        
        return new BaseApiResponse<ChatSession>
        {
            Data = null,
            Success = false,
            ResponseCode = ResponseCodes.NotFound,
            Message = "Chat session not found"
        };
    }
}