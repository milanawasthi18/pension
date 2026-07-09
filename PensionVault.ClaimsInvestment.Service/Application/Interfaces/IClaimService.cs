using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;

namespace PensionVault.ClaimsInvestment.Service.Application.Interfaces;

public interface IClaimService
{
    Task<ClaimResponse> SubmitClaimAsync(CreateClaimRequest request);
    Task<ClaimResponse> GetClaimAsync(Guid claimId);
    Task<IEnumerable<ClaimResponse>> GetAllClaimsAsync();
    Task<IEnumerable<ClaimResponse>> GetClaimsByMemberAsync(Guid memberId);
    Task<ClaimResponse> ReviewClaimAsync(Guid claimId, ClaimActionRequest request);
    Task<ClaimResponse> ApproveClaimAsync(Guid claimId, ClaimActionRequest request);
    Task<ClaimResponse> RejectClaimAsync(Guid claimId, ClaimActionRequest request);
    Task<DisbursementResponse> DisburseClaimAsync(Guid claimId, DisburseClaimRequest request);
    Task<ClaimResponse> SubmitPartialWithdrawalAsync(CreatePartialWithdrawalRequest request);
    Task<DisbursementResponse> DisbursePartialWithdrawalAsync(Guid claimId, DisbursePartialWithdrawalRequest request);
}
