using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;
using PensionVault.ClaimsInvestment.Service.Domain.Enums;

namespace PensionVault.ClaimsInvestment.Service.HttpClients;

public class FundOpsClient
{
    private readonly HttpClient _client;
    public FundOpsClient(HttpClient client) => _client = client;

    public async Task WriteLedgerEntryAsync(Guid accountId, string entryType, decimal amount, decimal balanceAfter, string? referenceId)
    {
        try
        {
            var response = await _client.PostAsJsonAsync("/internal/ledger/entries", new
            {
                accountId,
                entryType,
                amount,
                balanceAfter,
                referenceId
            });
            response.EnsureSuccessStatusCode();
        }
        catch { /* Fail silently – ledger writes are best effort */ }
    }
}
