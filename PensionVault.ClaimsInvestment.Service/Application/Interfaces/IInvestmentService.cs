using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PensionVault.ClaimsInvestment.Service.Application.Dtos;

namespace PensionVault.ClaimsInvestment.Service.Application.Interfaces;

public interface IInvestmentService
{
    Task<PortfolioResponse> CreatePortfolioAsync(CreatePortfolioRequest request);
    Task<PortfolioResponse> GetPortfolioAsync(Guid portfolioId);
    Task<IEnumerable<PortfolioResponse>> GetAllPortfoliosAsync(Guid? schemeId = null);
    Task<PortfolioResponse> UpdatePortfolioAsync(Guid portfolioId, UpdatePortfolioRequest request);
    Task<CorpusResponse> CreateCorpusRecordAsync(CreateCorpusRequest request);
    Task<CorpusResponse> GetCorpusRecordAsync(Guid corpusId);
    Task<IEnumerable<CorpusResponse>> GetCorpusHistoryAsync(Guid? schemeId = null);
    Task<CorpusResponse> FinaliseCorpusAsync(Guid corpusId);
}
