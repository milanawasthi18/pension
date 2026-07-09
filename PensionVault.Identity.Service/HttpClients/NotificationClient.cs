using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.HttpClients;

public class NotificationClient
{
    private readonly HttpClient _client;
    public NotificationClient(HttpClient client) => _client = client;

    public async Task SendNotificationAsync(Guid userId, string message, string category)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/internal/notifications", new { userId, message, category });
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Fail silently or log (notification should not block core operations)
        }
    }
}
