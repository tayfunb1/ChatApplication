using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DTOs.Response;

namespace ChatApplication.Business.Abstract;

public interface IChatSessionService
{
    Task<BaseApiResponse<ChatSessionResponseDto>> StartChatSessionAsync(int shiftId);
    Task<BaseApiListResponse<GetQueueDataResponseDto>> DisplayAssignedChatSessions();
}