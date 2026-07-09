using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Application.Dtos;

namespace PensionVault.FundOps.Service.Application.Interfaces;

public interface IContributionService
{
    Task<RemittanceResponse> CreateRemittanceAsync(CreateRemittanceRequest request);
    Task<RemittanceResponse> GetRemittanceAsync(Guid remittanceId);
    Task<IEnumerable<RemittanceResponse>> GetEmployerRemittancesAsync(Guid employerId);
    Task<RemittanceResponse> ReconcileAsync(Guid remittanceId);
    Task<IEnumerable<MemberContributionResponse>> GetMemberContributionsAsync(Guid memberId);
    Task<IEnumerable<RemittanceResponse>> GetAllRemittancesAsync();
    Task<ReconciliationReportResponse> GetReconciliationReportAsync(Guid remittanceId);
    Task<IEnumerable<RemittanceResponse>> GetDefaultersAsync();
    Task<IEnumerable<RemittanceResponse>> GetOverdueRemittancesAsync();
    Task<DefaulterSummaryResponse> GetDefaulterSummaryAsync(Guid employerId);
}
