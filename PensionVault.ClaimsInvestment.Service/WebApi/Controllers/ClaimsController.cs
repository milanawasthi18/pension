using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;
using PensionVault.ClaimsInvestment.Service.Application.Interfaces;

namespace PensionVault.ClaimsInvestment.Service.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly IClaimService _claimService;

    public ClaimsController(IClaimService claimService) => _claimService = claimService;

    [HttpPost]
    public async Task<ActionResult<ClaimResponse>> Submit([FromBody] CreateClaimRequest request) =>
        Ok(await _claimService.SubmitClaimAsync(request));

    [HttpGet("{claimId:guid}")]
    public async Task<ActionResult<ClaimResponse>> Get(Guid claimId) =>
        Ok(await _claimService.GetClaimAsync(claimId));

    [HttpGet("member/{memberId:guid}")]
    public async Task<ActionResult<IEnumerable<ClaimResponse>>> GetByMember(Guid memberId) =>
        Ok(await _claimService.GetClaimsByMemberAsync(memberId));

    [HttpPut("{claimId:guid}/review")]
    public async Task<ActionResult<ClaimResponse>> Review(Guid claimId, [FromBody] ClaimActionRequest request) =>
        Ok(await _claimService.ReviewClaimAsync(claimId, request));

    [HttpPut("{claimId:guid}/approve")]
    public async Task<ActionResult<ClaimResponse>> Approve(Guid claimId, [FromBody] ClaimActionRequest request) =>
        Ok(await _claimService.ApproveClaimAsync(claimId, request));

    [HttpPut("{claimId:guid}/reject")]
    public async Task<ActionResult<ClaimResponse>> Reject(Guid claimId, [FromBody] ClaimActionRequest request) =>
        Ok(await _claimService.RejectClaimAsync(claimId, request));

    [HttpPost("{claimId:guid}/disburse")]
    public async Task<ActionResult<DisbursementResponse>> Disburse(Guid claimId, [FromBody] DisburseClaimRequest request) =>
        Ok(await _claimService.DisburseClaimAsync(claimId, request));

    [HttpPost("partial-withdrawal")]
    public async Task<ActionResult<ClaimResponse>> SubmitPartial([FromBody] CreatePartialWithdrawalRequest request) =>
        Ok(await _claimService.SubmitPartialWithdrawalAsync(request));

    [HttpPost("partial-withdrawal/{claimId:guid}/disburse")]
    public async Task<ActionResult<DisbursementResponse>> DisbursePartial(Guid claimId, [FromBody] DisbursePartialWithdrawalRequest request) =>
        Ok(await _claimService.DisbursePartialWithdrawalAsync(claimId, request));
}
