using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DTOs.Response;

namespace ChatApplication.Business.Abstract;

public interface IChatSessionService
{
    Task<BaseApiResponse<ChatSessionResponse>> StartChatSessionAsync(int shiftId);
    Task<BaseApiListResponse<GetQueueDataResponse>> DisplayAssignedChatSessions();
}