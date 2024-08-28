using ChatApplication.DataAccess.Entities;

namespace ChatApplication.Business.Abstract;

public interface ITeamService
{
    Team GetActiveTeam(int shiftId);
}