using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace PensionVault.EmployerMember.Service.HttpClients;

public class FundOpsClient
{
    private readonly HttpClient _client;
    public FundOpsClient(HttpClient client) => _client = client;

    public async Task<IEnumerable<object>> GetMemberContributionsAsync(Guid memberId)
    {
        try
        {
            return await _client.GetFromJsonAsync<IEnumerable<object>>($"/api/remittances/member/{memberId}") ?? Array.Empty<object>();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }

    public async Task<IEnumerable<object>> GetLedgerByAccountAsync(Guid accountId)
    {
        try
        {
            return await _client.GetFromJsonAsync<IEnumerable<object>>($"/api/ledger/account/{accountId}") ?? Array.Empty<object>();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }

    public async Task<IEnumerable<object>> GetRemittancesByEmployerAsync(Guid employerId)
    {
        try
        {
            return await _client.GetFromJsonAsync<IEnumerable<object>>($"/api/remittances?employerId={employerId}") ?? Array.Empty<object>();
        }
        catch
        {
            return Array.Empty<object>();
        }
    }
}
