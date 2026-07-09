using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace PensionVault.FundOps.Service.HttpClients;

public class JwtForwardingHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _ctx;
    public JwtForwardingHandler(IHttpContextAccessor ctx) => _ctx = ctx;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var token = _ctx.HttpContext?.Request.Headers["Authorization"].ToString();
        if (!string.IsNullOrEmpty(token))
            request.Headers.TryAddWithoutValidation("Authorization", token);
        return base.SendAsync(request, ct);
    }
}
