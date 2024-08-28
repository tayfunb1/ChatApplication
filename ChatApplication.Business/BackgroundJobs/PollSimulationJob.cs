using ChatApplication.Business.Models.Common;
using ChatApplication.DataAccess.Abstract;
using ChatApplication.DataAccess.Entities;
using Newtonsoft.Json;

namespace ChatApplication.Business.BackgroundJobs;

public class PollSimulationJob(IChatQueueManager chatQueueManager, HttpClient httpClient) : Microsoft.Extensions.Hosting.BackgroundService
{
    private const string ApiUrl = "http://localhost:5220/api/Chat/poll";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var chatsInQueue = await chatQueueManager.DisplayQueueData();
            var chatsToBePolled = 
                chatsInQueue.Where(x => x.IsToBePolled).ToList();
            if (chatsToBePolled.Count > 0)
            {
                // "Once the chat window receives OK as a response it will start polling every 1s"
                await Parallel.ForEachAsync(chatsToBePolled, stoppingToken, async (chatSession, token) =>
                {
                    var request = $"{ApiUrl}/{chatSession.Id}";
                    var response = await httpClient.PostAsync(request, null, token);
                    response.EnsureSuccessStatusCode();

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync(token);
                        var result = JsonConvert.DeserializeObject<BaseApiResponse<ChatSession>>(responseContent);
                        Console.WriteLine(result.Message);
                    }

                    await Task.Delay(1000, token);
                });
            }
            
            else
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}