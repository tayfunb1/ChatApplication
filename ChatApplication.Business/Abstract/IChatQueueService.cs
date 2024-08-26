using ChatApplication.Business.Models.DataAccess.Entities;
using ChatApplication.Business.Models.DTOs.Common;
using ChatApplication.Business.Models.DTOs.Response;

namespace ChatApplication.Business.Abstract;

public interface IChatQueueService
{
    Task<bool> QueueChatSession(ChatSession session, Team activeTeam);
    Task<ChatSession> DequeueChatSessionWithCondition(Func<ChatSession, bool> condition);
    Task<BaseApiListResponse<GetQueueDataResponse>> DisplayQueueData();
    Task<BaseApiResponse<ChatSession>> PollChatSession(Guid chatSessionId);
}