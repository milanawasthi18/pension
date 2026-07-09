using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;
using PensionVault.ClaimsInvestment.Service.Application.Interfaces;
using PensionVault.ClaimsInvestment.Service.Domain.Models;
using PensionVault.ClaimsInvestment.Service.Domain.Enums;
using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.HttpClients;

namespace PensionVault.ClaimsInvestment.Service.Application.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _investmentRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly EmployerMemberClient _employerMemberClient;

    public InvestmentService(
        IInvestmentRepository investmentRepo,
        IUnitOfWork unitOfWork,
        EmployerMemberClient employerMemberClient)
    {
        _investmentRepo = investmentRepo;
        _unitOfWork = unitOfWork;
        _employerMemberClient = employerMemberClient;
    }

    public async Task<IEnumerable<PortfolioResponse>> GetAllPortfoliosAsync(Guid? schemeId = null)
    {
        var portfolios = await _investmentRepo.GetPortfoliosAsync(schemeId);
        var responses = new List<PortfolioResponse>();
        foreach (var p in portfolios)
        {
            var scheme = await _employerMemberClient.GetSchemeAsync(p.SchemeId);
            responses.Add(ToPortfolioResponse(p, scheme?.SchemeName ?? "Unknown"));
        }
        return responses;
    }

    public async Task<PortfolioResponse> GetPortfolioAsync(Guid portfolioId)
    {
        var p = await _investmentRepo.FindPortfolioByIdAsync(portfolioId)
            ?? throw new KeyNotFoundException("Portfolio not found.");
        var scheme = await _employerMemberClient.GetSchemeAsync(p.SchemeId);
        return ToPortfolioResponse(p, scheme?.SchemeName ?? "Unknown");
    }

    public async Task<PortfolioResponse> CreatePortfolioAsync(CreatePortfolioRequest request)
    {
        var portfolio = new InvestmentPortfolio
        {
            SchemeId = request.SchemeId,
            AssetClass = request.AssetClass,
            AllocationPercent = request.AllocationPercent,
            InvestedValue = request.InvestedValue,
            CurrentValue = request.CurrentValue,
            YieldEarned = request.YieldEarned,
            LastUpdated = DateTime.UtcNow
        };
        await _investmentRepo.AddPortfolioAsync(portfolio);
        await _unitOfWork.SaveChangesAsync();
        return await GetPortfolioAsync(portfolio.PortfolioId);
    }

    public async Task<PortfolioResponse> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request)
    {
        var portfolio = await _investmentRepo.FindPortfolioByIdAsync(portfolioId)
            ?? throw new KeyNotFoundException("Portfolio not found.");
        portfolio.AllocationPercent = request.AllocationPercent;
        portfolio.InvestedValue = request.InvestedValue;
        portfolio.CurrentValue = request.CurrentValue;
        portfolio.YieldEarned = request.YieldEarned;
        portfolio.LastUpdated = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync();
        var scheme = await _employerMemberClient.GetSchemeAsync(portfolio.SchemeId);
        return ToPortfolioResponse(portfolio, scheme?.SchemeName ?? "Unknown");
    }

    public async Task<IEnumerable<CorpusResponse>> GetCorpusHistoryAsync(Guid? schemeId = null)
    {
        var records = await _investmentRepo.GetCorpusRecordsAsync(schemeId);
        var responses = new List<CorpusResponse>();
        foreach (var c in records)
        {
            var scheme = await _employerMemberClient.GetSchemeAsync(c.SchemeId);
            var opening = c.ClosingCorpus - c.TotalContributions + c.TotalWithdrawals
                          - c.InvestmentIncome + c.ManagementExpenses;
            responses.Add(ToCorpusResponse(c, scheme?.SchemeName ?? "Unknown", opening));
        }
        return responses;
    }

    public async Task<CorpusResponse> GetCorpusRecordAsync(Guid corpusId)
    {
        var c = await _investmentRepo.FindCorpusByIdAsync(corpusId)
            ?? throw new KeyNotFoundException("Corpus record not found.");
        var scheme = await _employerMemberClient.GetSchemeAsync(c.SchemeId);
        var opening = c.ClosingCorpus - c.TotalContributions + c.TotalWithdrawals
                      - c.InvestmentIncome + c.ManagementExpenses;
        return ToCorpusResponse(c, scheme?.SchemeName ?? "Unknown", opening);
    }

    public async Task<CorpusResponse> CreateCorpusRecordAsync(CreateCorpusRequest request)
    {
        var lastCorpus = await _investmentRepo.GetLastFinalisedCorpusAsync(request.SchemeId);
        var openingCorpus = lastCorpus?.ClosingCorpus ?? 0;

        var corpus = new CorpusRecord
        {
            SchemeId = request.SchemeId,
            RecordDate = request.RecordDate,
            TotalContributions = request.TotalContributions,
            TotalWithdrawals = request.TotalWithdrawals,
            InvestmentIncome = request.InvestmentIncome,
            ManagementExpenses = request.ManagementExpenses,
            ClosingCorpus = openingCorpus + request.TotalContributions - request.TotalWithdrawals
                + request.InvestmentIncome - request.ManagementExpenses,
            Status = CorpusStatus.Draft
        };
        await _investmentRepo.AddCorpusAsync(corpus);
        await _unitOfWork.SaveChangesAsync();
        return await GetCorpusRecordAsync(corpus.CorpusId);
    }

    public async Task<CorpusResponse> FinaliseCorpusAsync(Guid corpusId)
    {
        var corpus = await _investmentRepo.FindCorpusByIdAsync(corpusId)
            ?? throw new KeyNotFoundException("Corpus record not found.");
        corpus.Status = CorpusStatus.Finalised;
        await _unitOfWork.SaveChangesAsync();
        return await GetCorpusRecordAsync(corpusId);
    }

    private static PortfolioResponse ToPortfolioResponse(InvestmentPortfolio p, string schemeName) => new(
        p.PortfolioId, p.SchemeId, schemeName,
        p.AssetClass, p.AllocationPercent, p.InvestedValue,
        p.CurrentValue, p.YieldEarned, p.LastUpdated);

    private static CorpusResponse ToCorpusResponse(CorpusRecord c, string schemeName, decimal opening) => new(
        c.CorpusId, c.SchemeId, schemeName, c.RecordDate,
        opening, c.TotalContributions, c.TotalWithdrawals,
        c.InvestmentIncome, c.ManagementExpenses, c.ClosingCorpus, c.Status);
}
