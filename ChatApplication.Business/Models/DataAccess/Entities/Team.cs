using ChatApplication.Business.Models.DataAccess.Entities.Enums;

namespace ChatApplication.Business.Models.DataAccess.Entities;

public class Team
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<Agent> Agents { get; set; }
    public Shift Shift { get; set; }
    
    public int CalculateCapacity()
    {
        return Agents.Sum(agent => agent.CalculateCapacity());
    }

    public int CalculateMaxQueueLength()
    {
        return (int)(CalculateCapacity() * 1.5);
    }
}