using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.HttpClients;

public class IdentityClient
{
    private readonly HttpClient _client;
    public IdentityClient(HttpClient client) => _client = client;

    public async Task<UserInfo?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _client.GetFromJsonAsync<UserInfo>($"/internal/users/{userId}");
        }
        catch { return null; }
    }

    public async Task<List<Guid>> GetUsersByRoleAsync(string role)
    {
        try
        {
            var users = await _client.GetFromJsonAsync<List<UserInfo>>($"/internal/users/by-org/{Guid.Empty}/role/{role}");
            return users?.ConvertAll(u => u.UserId) ?? new();
        }
        catch { return new(); }
    }

    public record UserInfo(Guid UserId, string Name, string Email, string Role, Guid? OrganisationId, string? EmployeeId, string? Phone);
}
