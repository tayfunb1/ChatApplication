using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DTOs.Response;
using ChatApplication.DataAccess.Entities;

namespace ChatApplication.Business.Abstract;

public interface IChatQueueService
{
    Task<BaseApiListResponse<GetQueueDataResponseDto>> DisplayQueueData();
    Task<BaseApiResponse<ChatSession>> PollChatSession(Guid chatSessionId);
}