using ChatApplication.Business.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace ChatApplication.Controllers;

public class ChatController(IChatSessionService chatSessionService, IChatQueueService chatQueueService) : BaseController
{
    /// <summary>
    /// Queues a chat session
    /// </summary>
    /// <param name="shiftId">This is only to simulate using different teams,
    /// since it wouldn't be possible to wait 8 hours for each team while testing.
    /// Morning Shift(Team A) = 0, Afternoon Shift(Team B) = 1, Night Shift(Team C) = 2</param>
    /// <returns>ID and Date of the created session</returns>
    [HttpPost("sessions/start/{shiftId:int}")]
    public async Task<IActionResult> StartChatSession(int shiftId)
    {
        var response = await chatSessionService.StartChatSessionAsync(shiftId);
        return BuildResponse(response);
    }
    
    /// <summary>
    /// Displays chat sessions that have been assigned to agents
    /// </summary>
    /// <returns>Team and agent data of the assigned chat sessions</returns>
    [HttpGet("sessions/assigned")]
    public async Task<IActionResult> DisplayAssignedChatSessions()
    {
        var response = await chatSessionService.DisplayAssignedChatSessions();
        return BuildResponse(response);
    }
    
    /// <summary>
    /// Displays data inside current queue
    /// </summary>
    /// <returns>Data of the unassigned chat sessions that are either failed on polling or
    /// passed polling and waiting for assignment</returns>
    [HttpGet("queue")]
    public async Task<IActionResult> DisplayQueueData()
    {
        var response = await chatQueueService.DisplayQueueData();
        return BuildResponse(response);
    }
    
    /// <summary>
    /// Polls a chat session based on the provided Id. This endpoint is only used to simulate client-side polling,
    /// this is achieved by a background service(PollSimulationJob) in this project.
    /// </summary>
    /// <param name="chatSessionId">Id to be polled</param>
    /// <returns>Data of the polled chat session</returns>
    [HttpPost("poll/{chatSessionId:guid}")]
    public async Task<IActionResult> PollChatSession(Guid chatSessionId)
    {
        var response = await chatQueueService.PollChatSession(chatSessionId);
        return BuildResponse(response);
    }
}