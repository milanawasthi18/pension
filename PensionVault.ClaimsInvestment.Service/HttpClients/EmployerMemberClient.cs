using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PensionVault.ClaimsInvestment.Service.HttpClients;

public class EmployerMemberClient
{
    private readonly HttpClient _client;
    public EmployerMemberClient(HttpClient client) => _client = client;

    public async Task<MemberDto?> GetMemberAsync(Guid memberId)
    {
        try { return await _client.GetFromJsonAsync<MemberDto>($"/internal/members/{memberId}"); }
        catch { return null; }
    }

    public async Task<FundAccountDto?> GetActiveFundAccountAsync(Guid memberId)
    {
        try { return await _client.GetFromJsonAsync<FundAccountDto>($"/internal/members/{memberId}/fund-account"); }
        catch { return null; }
    }

    public async Task<FundAccountDto?> UpdateBalanceAsync(Guid accountId, decimal delta, decimal employeeDelta, decimal employerDelta, decimal interestDelta = 0)
    {
        try
        {
            var response = await _client.PutAsJsonAsync(
                $"/internal/members/fund-accounts/{accountId}/balance",
                new { delta, employeeDelta, employerDelta, interestDelta });
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<FundAccountDto>();
        }
        catch { return null; }
    }

    public async Task<SchemeDto?> GetSchemeAsync(Guid schemeId)
    {
        try { return await _client.GetFromJsonAsync<SchemeDto>($"/internal/schemes/{schemeId}"); }
        catch { return null; }
    }

    public record MemberDto(Guid MemberId, Guid UserId, string Name, string MembershipNumber, Guid EmployerId);
    public record FundAccountDto(Guid AccountId, Guid MemberId, Guid SchemeId, string SchemeName, decimal TotalBalance, decimal VestingPercent, string Status);
    public record SchemeDto(Guid SchemeId, string SchemeName, string SchemeType);
}
