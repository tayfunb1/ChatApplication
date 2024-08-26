using ChatApplication.Business.Models.DataAccess.Entities;

namespace ChatApplication.Business.Abstract;

public interface ITeamService
{
    Team GetActiveTeam(int shiftId);
}