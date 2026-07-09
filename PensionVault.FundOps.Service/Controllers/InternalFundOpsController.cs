using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PensionVault.FundOps.Service.Application.Dtos;
using PensionVault.FundOps.Service.Application.Interfaces;
using PensionVault.FundOps.Service.Domain.Enums;
using PensionVault.FundOps.Service.Domain.Interfaces;

namespace PensionVault.FundOps.Service.Controllers;

[ApiController]
[Route("internal")]
public class InternalFundOpsController : ControllerBase
{
    private readonly ILedgerService _ledgerService;
    private readonly ILedgerRepository _ledgerRepo;

    public InternalFundOpsController(ILedgerService ledgerService, ILedgerRepository ledgerRepo)
    {
        _ledgerService = ledgerService;
        _ledgerRepo = ledgerRepo;
    }

    [HttpPost("ledger/entries")]
    public async Task<IActionResult> WriteLedgerEntry([FromBody] LedgerEntryRequest request)
    {
        try
        {
            await _ledgerService.WriteLedgerEntryAsync(request);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("ledger/account/{accountId:guid}/sum/{entryType}")]
    public async Task<IActionResult> GetSumByType(Guid accountId, string entryType)
    {
        if (!Enum.TryParse<EntryType>(entryType, true, out var parsedType))
            return BadRequest("Invalid entry type.");

        var sum = await _ledgerRepo.SumByTypeAsync(accountId, parsedType);
        return Ok(sum);
    }
}
