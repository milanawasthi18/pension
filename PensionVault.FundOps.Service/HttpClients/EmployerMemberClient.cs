using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Application.Dtos;

namespace PensionVault.FundOps.Service.HttpClients;

public class EmployerMemberClient
{
    private readonly HttpClient _client;
    public EmployerMemberClient(HttpClient client) => _client = client;

    public async Task<bool> ValidateEmployerAsync(Guid employerId)
    {
        try
        {
            var response = await _client.GetAsync($"/internal/employers/{employerId}");
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<EmployerDto?> GetEmployerAsync(Guid employerId)
    {
        try
        {
            return await _client.GetFromJsonAsync<EmployerDto>($"/internal/employers/{employerId}");
        }
        catch { return null; }
    }

    public async Task<List<UserSummaryDto>> GetEmployerUsersAsync(Guid employerId)
    {
        try
        {
            return await _client.GetFromJsonAsync<List<UserSummaryDto>>($"/internal/employers/{employerId}/users") ?? new();
        }
        catch { return new(); }
    }

    public async Task<MemberDto?> GetMemberAsync(Guid memberId)
    {
        try
        {
            return await _client.GetFromJsonAsync<MemberDto>($"/internal/members/{memberId}");
        }
        catch { return null; }
    }

    public async Task<FundAccountResponse?> GetActiveFundAccountAsync(Guid memberId)
    {
        try
        {
            return await _client.GetFromJsonAsync<FundAccountResponse>($"/internal/members/{memberId}/fund-account");
        }
        catch { return null; }
    }

    public async Task<FundAccountResponse?> UpdateBalanceAsync(Guid accountId, decimal delta, decimal employeeDelta, decimal employerDelta, decimal interestDelta = 0)
    {
        var response = await _client.PutAsJsonAsync($"/internal/members/fund-accounts/{accountId}/balance", new { delta, employeeDelta, employerDelta, interestDelta });
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<FundAccountResponse>();
    }

    public record EmployerDto(Guid EmployerId, string CompanyName);
    public record MemberDto(Guid MemberId, Guid UserId, string Name, string MembershipNumber);
    public record UserSummaryDto(Guid UserId, string Name, string Email);
}
