using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Application.Dtos;

namespace PensionVault.FundOps.Service.Application.Interfaces;

public interface ILedgerService
{
    Task<IEnumerable<LedgerEntryResponse>> GetAccountLedgerAsync(Guid accountId);
    Task<IEnumerable<LedgerEntryResponse>> GetAllLedgerEntriesAsync();
    Task<InterestCreditResponse> CreditInterestAsync(CreditInterestRequest request);
    Task<IEnumerable<InterestCreditResponse>> GetInterestRecordsAsync(Guid accountId);
    Task WriteLedgerEntryAsync(LedgerEntryRequest request); // S2S helper
}
