using ChatApplication.Business.Abstract;
using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DataAccess.Entities;
using ChatApplication.Business.Models.DTOs.Response;
using ChatApplication.Business.Utility;

namespace ChatApplication.Business.Concrete;

public class ChatQueueService : IChatQueueService
{
    private readonly LimitedConcurrentQueue<ChatSession> _queue = new();
    private const int PollThreshold = 3;

    //"Once a chat session is created, it is put in an FIFO queue and monitored."
    public Task<bool> QueueChatSession(ChatSession session, Team activeTeam)
    {
        var queueLimit = activeTeam.CalculateMaxQueueLength();
        session.AssignedTeam = activeTeam;
        var result = _queue.TryEnqueue(session, queueLimit);
        return Task.FromResult(result);
    }
    
    public Task<ChatSession> DequeueChatSessionWithCondition(Func<ChatSession, bool> condition)
    {
        return Task.FromResult(_queue.DequeueOnCondition(out var session, condition) ? session : null);
    }

    public Task<BaseApiListResponse<GetQueueDataResponse>> DisplayQueueData()
    {
        var result = _queue.GetQueueData();
        return Task.FromResult(new BaseApiListResponse<GetQueueDataResponse>
        {
            Message = "Queue data acquired successfully",
            DataList = result.Select(x => new GetQueueDataResponse
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
        });
    }

    public Task<BaseApiResponse<ChatSession>> PollChatSession(Guid chatSessionId)
    {
        var session = _queue.GetQueueData(x => x.Id == chatSessionId).SingleOrDefault();
        if (session is not null)
        {
            if (session.CreatedAt.AddSeconds(3) < DateTime.UtcNow)
            {
                session.IsToBePolled = false;
                return Task.FromResult(new BaseApiResponse<ChatSession>
                {
                    Data = session,
                    Success = true,
                    ResponseCode = ResponseCodes.Ok,
                    Message = $"Poll unsuccessful, Session Created Time:{session.CreatedAt.TimeOfDay}, Poll Time: {DateTime.UtcNow.TimeOfDay}"
                });
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

            return Task.FromResult(new BaseApiResponse<ChatSession>
            {
                Data = session,
                Success = true,
                ResponseCode = ResponseCodes.Ok,
                Message = $"Chat session with Id: {session.Id} has been polled"
            });
        }

        return Task.FromResult(new BaseApiResponse<ChatSession>
        {
            Data = null,
            Success = false,
            ResponseCode = ResponseCodes.NotFound,
            Message = "Chat session not found"
        });
    }
}