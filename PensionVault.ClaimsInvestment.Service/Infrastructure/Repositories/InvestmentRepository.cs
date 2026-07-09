using Microsoft.EntityFrameworkCore;
using PensionVault.ClaimsInvestment.Service.Domain.Models;
using PensionVault.ClaimsInvestment.Service.Domain.Interfaces;
using PensionVault.ClaimsInvestment.Service.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PensionVault.ClaimsInvestment.Service.Infrastructure.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly ClaimsInvestmentDbContext _context;
    public InvestmentRepository(ClaimsInvestmentDbContext context) => _context = context;

    public Task<InvestmentPortfolio?> FindPortfolioByIdAsync(Guid portfolioId)
        => _context.InvestmentPortfolios
            .FirstOrDefaultAsync(p => p.PortfolioId == portfolioId);

    public Task<List<InvestmentPortfolio>> GetPortfoliosAsync(Guid? schemeId = null)
    {
        var query = _context.InvestmentPortfolios.AsQueryable();
        if (schemeId.HasValue) query = query.Where(p => p.SchemeId == schemeId);
        return query.ToListAsync();
    }

    public async Task AddPortfolioAsync(InvestmentPortfolio portfolio)
        => await _context.InvestmentPortfolios.AddAsync(portfolio);

    public Task<CorpusRecord?> FindCorpusByIdAsync(Guid corpusId)
        => _context.CorpusRecords
            .FirstOrDefaultAsync(c => c.CorpusId == corpusId);

    public Task<List<CorpusRecord>> GetCorpusRecordsAsync(Guid? schemeId = null)
    {
        var query = _context.CorpusRecords.AsQueryable();
        if (schemeId.HasValue) query = query.Where(c => c.SchemeId == schemeId);
        return query.OrderByDescending(c => c.RecordDate).ToListAsync();
    }

    public Task<CorpusRecord?> GetLastFinalisedCorpusAsync(Guid schemeId)
        => _context.CorpusRecords
            .Where(c => c.SchemeId == schemeId && c.Status == Domain.Enums.CorpusStatus.Finalised)
            .OrderByDescending(c => c.RecordDate)
            .FirstOrDefaultAsync();

    public async Task AddCorpusAsync(CorpusRecord corpus)
        => await _context.CorpusRecords.AddAsync(corpus);
}
