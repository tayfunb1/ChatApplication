using ChatApplication.Business.Abstract;
using ChatApplication.DataAccess.Entities;

namespace ChatApplication.Business.Concrete;

public class TeamService(IEnumerable<Team> teams, OverflowTeam overflowTeam) : ITeamService
{
    private readonly TimeSpan _officeHoursStart = new(9, 0, 0);
    private readonly TimeSpan _officeHoursEnd = new(17, 0, 0);
    
    public Team GetActiveTeam(int shiftId)
    {
        var team = teams.FirstOrDefault(t => (int)t.Shift == shiftId && t.Agents.Any(x => x.IsAvailable));
        if (team is not null)
        {
            var result = new Team
            {
                Name = team.Name,
                Shift = team.Shift,
                Agents = [..team.Agents]
            };
            
            var now = DateTime.UtcNow.TimeOfDay;
            var isOfficeHours = now >= _officeHoursStart && now <= _officeHoursEnd;
            
            // "Once the maximum queue is reached and if during office hours, an overflow team kicks in.
            // This adds extra people who are not normally their job to work as such."
            if (isOfficeHours)
            {
                result.Agents.AddRange(overflowTeam.Agents);
            }
            
            return result;
        }

        return null;
    }
}