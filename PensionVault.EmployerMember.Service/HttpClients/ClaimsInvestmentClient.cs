using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.HttpClients;

public class ClaimsInvestmentClient
{
    private readonly HttpClient _client;
    public ClaimsInvestmentClient(HttpClient client) => _client = client;

    public async Task<IEnumerable<object>> GetClaimsByMemberAsync(Guid memberId)
    {
        try
        {
            return await _client.GetFromJsonAsync<IEnumerable<object>>($"/internal/claims?memberId={memberId}") ?? Array.Empty<object>();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }
}
