using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Entities;
using ChatApplication.DataAccess.Utility;

namespace ChatApplication.DataAccess.Concrete;

public class ChatQueueManager : IChatQueueManager
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

    public Task<IReadOnlyCollection<ChatSession>> DisplayQueueData()
    {
        var result = _queue.GetQueueData();
        return Task.FromResult(result);
    }

    public Task<ChatSession> PollChatSession(Guid chatSessionId)
    {
        var session = _queue.GetQueueData(x => x.Id == chatSessionId).SingleOrDefault();
        if (session is not null)
        {
            if (session.CreatedAt.AddSeconds(3) < DateTime.UtcNow)
            {
                session.IsToBePolled = false;
                return Task.FromResult(session);
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

            return Task.FromResult(session);
        }

        return Task.FromResult<ChatSession>(null);
    }
}