using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.Identity.Service.HttpClients;

public class EmployerMemberClient
{
    private readonly HttpClient _client;
    public EmployerMemberClient(HttpClient client) => _client = client;

    public async Task<Guid> CreateEmployerForUserAsync(string name, string email)
    {
        var response = await _client.PostAsJsonAsync("/internal/employers", new { companyName = name + " Corporation", contactDetails = email });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<EmployerCreateResult>();
        return result?.EmployerId ?? throw new Exception("Failed to parse employer creation result");
    }

    private record EmployerCreateResult(Guid EmployerId);
}
