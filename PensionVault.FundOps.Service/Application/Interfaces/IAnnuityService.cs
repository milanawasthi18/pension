using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.FundOps.Service.Application.Dtos;

namespace PensionVault.FundOps.Service.Application.Interfaces;

public interface IAnnuityService
{
    Task<AnnuityResponse> CreateAnnuityAsync(CreateAnnuityRequest request);
    Task<AnnuityResponse> GetAnnuityAsync(Guid annuityId);
    Task<IEnumerable<PensionDisbursementResponse>> GetDisbursementsAsync(Guid annuityId);
    Task<PensionDisbursementResponse> ProcessDisbursementAsync(ProcessDisbursementRequest request);
    Task<IEnumerable<AnnuityResponse>> GetAllAnnuitiesAsync();
    Task<AnnuityResponse> ProcessNomineeSettlementAsync(Guid annuityId, NomineeSettlementRequest request);
    Task<AnnuityResponse> TerminateAnnuityAsync(Guid annuityId);
}
