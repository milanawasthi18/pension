using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Domain.Models;
using PensionVault.FundOps.Service.Domain.Enums;

namespace PensionVault.FundOps.Service.Domain.Interfaces;

public interface IContributionRepository
{
    Task<ContributionRemittance?> FindRemittanceByIdAsync(Guid remittanceId);
    Task<List<ContributionRemittance>> GetAllRemittancesAsync();
    Task<List<ContributionRemittance>> GetByEmployerAsync(Guid employerId);
    Task<List<ContributionRemittance>> GetByStatusesAsync(params RemittanceStatus[] statuses);
    Task<int> CountPostedContributionsAsync(Guid remittanceId);
    Task<decimal> SumReconciledAmountAsync(Guid remittanceId);
    Task<List<MemberContribution>> GetByMemberAsync(Guid memberId);
    Task AddRemittanceAsync(ContributionRemittance remittance);
    Task AddContributionAsync(MemberContribution contribution);
}
