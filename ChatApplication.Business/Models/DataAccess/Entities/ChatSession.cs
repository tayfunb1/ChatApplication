namespace ChatApplication.Business.Models.DataAccess.Entities;

public class ChatSession
{
    public Guid Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
    public Team AssignedTeam { get; set; }
    public int PollCount { get; set; }
    public bool IsToBePolled { get; set; } = true;
}
