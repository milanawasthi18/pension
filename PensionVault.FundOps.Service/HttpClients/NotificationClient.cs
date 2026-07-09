using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.FundOps.Service.HttpClients;

public class NotificationClient
{
    private readonly HttpClient _client;
    public NotificationClient(HttpClient client) => _client = client;

    public async Task SendAsync(Guid userId, string message, string category)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/internal/notifications", new { userId, message, category });
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Fail silently
        }
    }

    public async Task SendBatchAsync(List<NotificationItem> items)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/internal/notifications/batch", new { items });
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Fail silently
        }
    }

    public record NotificationItem(Guid UserId, string Message, string Category);
}
