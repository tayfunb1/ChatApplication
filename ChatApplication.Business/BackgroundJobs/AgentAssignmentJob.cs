using System.Collections.Concurrent;
using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Entities;

namespace ChatApplication.Business.BackgroundJobs;

public class AgentAssignmentJob(IChatQueueManager chatQueueManager, 
    ConcurrentDictionary<Guid, ChatSession> assignedChatSessions) : Microsoft.Extensions.Hosting.BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        Func<ChatSession, bool> condition = session => session.IsActive && session.PollCount == 3;
        while (!stoppingToken.IsCancellationRequested)
        {
            var chatSession = await chatQueueManager.DequeueChatSessionWithCondition(condition);
            
            if (chatSession is not null && chatSession.IsActive && chatSession.PollCount == 3)
            {
                var assignedTeam = chatSession.AssignedTeam;
                var agent = assignedTeam.Agents
                    // "Chats are assigned in a round robin fashion,
                    // preferring to assign the junior first, then mid, then senior etc."
                    .OrderBy(a => a.AgentType)
                    .FirstOrDefault(a => a.IsAvailable);
                agent?.ChatSessions.Add(chatSession);
                assignedChatSessions[chatSession.Id] = chatSession;
            }
            
            await Task.Delay(5000, stoppingToken);
        }
    }
}