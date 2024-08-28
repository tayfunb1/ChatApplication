using ChatApplication.DataAccess.Entities;

namespace ChatApplication.DataAccess.Abstract;

public interface IChatQueueManager
{
    Task<bool> QueueChatSession(ChatSession session, Team activeTeam);
    Task<ChatSession> DequeueChatSessionWithCondition(Func<ChatSession, bool> condition);
    Task<IReadOnlyCollection<ChatSession>> DisplayQueueData();
    Task<ChatSession> PollChatSession(Guid chatSessionId);
}