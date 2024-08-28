using ChatApplication.DataAccess.Entities.Enums;

namespace ChatApplication.DataAccess.Entities;

public class Agent
{
    public string Name { get; set; }
    public AgentType AgentType { get; set; }

    public List<ChatSession> ChatSessions { get; set; } = [];
    public bool IsAvailable => ChatSessions.Count < CalculateCapacity();

    public int CalculateCapacity()
    {
        return (int)(10 * GetEfficiencyMultiplier(AgentType));
    }

    private double GetEfficiencyMultiplier(AgentType agentType)
    {
        return agentType switch
        {
            AgentType.Junior => 0.4,
            AgentType.MidLevel => 0.6,
            AgentType.Senior => 0.8,
            AgentType.TeamLead => 0.5,
            AgentType.Overflow => 0.4,
            _ => 0.4 // Default to Junior
        };
    }
}