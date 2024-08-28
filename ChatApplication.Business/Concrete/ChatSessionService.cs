using System.Collections.Concurrent;
using ChatApplication.Business.Abstract;
using ChatApplication.Business.Models.Common;
using ChatApplication.Business.Models.DataAccess.Entities;
using ChatApplication.Business.Models.DTOs.Response;

namespace ChatApplication.Business.Concrete;

public class ChatSessionService(IChatQueueService chatQueueService, ITeamService teamService, 
    ConcurrentDictionary<Guid, ChatSession> assignedChatSessions)
    : IChatSessionService
{
    public async Task<BaseApiResponse<ChatSessionResponse>> StartChatSessionAsync(int shiftId)
    {
        var chatSession = new ChatSession
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow
        };

        var activeTeam = teamService.GetActiveTeam(shiftId);
        if (activeTeam == null)
        {
            return new BaseApiResponse<ChatSessionResponse>
            {
                Data = null,
                Success = false,
                ResponseCode = ResponseCodes.InternalServerError,
                Message = "An error occured during team selection"
            };
        }

        var isChatQueued = await chatQueueService.QueueChatSession(chatSession, activeTeam);

        return isChatQueued
            ? new BaseApiResponse<ChatSessionResponse>
            {
                Data = new ChatSessionResponse
                {
                    Id = chatSession.Id,
                    CreatedAt = chatSession.CreatedAt
                },
                Success = true,
                ResponseCode = ResponseCodes.Ok,
                Message = "Chat session is queued successfully"
            }
            : new BaseApiResponse<ChatSessionResponse>
            {
                Data = new ChatSessionResponse(),
                Success = false,
                ResponseCode = ResponseCodes.ServiceUnavailable,
                Message = "No agents available at the moment, please try again later"
            };
    }

    public Task<BaseApiListResponse<GetQueueDataResponse>> DisplayAssignedChatSessions()
    {
        var result = new BaseApiListResponse<GetQueueDataResponse>
        {
            DataList = assignedChatSessions.Select(x => new GetQueueDataResponse
            {
                Id = x.Key,
                CreatedAt = x.Value.CreatedAt,
                IsActive = x.Value.IsActive,
                PollCount = x.Value.PollCount,
                IsToBePolled = x.Value.IsToBePolled,
                AssignedTeam = new ChatSessionTeamResponse
                {
                    Name = x.Value.AssignedTeam.Name,
                    AssignedAgent = x.Value.AssignedTeam.Agents.FirstOrDefault(y => y.ChatSessions.Contains(x.Value))?.Name
                }
            }).OrderBy(x => x.CreatedAt).ToList(),
            Success = true,
            ResponseCode = ResponseCodes.Ok,
            Message = "Assigned chat session data acquired successfully"
        };
        return Task.FromResult(result);
    }
}