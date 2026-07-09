using System.Threading.Tasks;
using PensionVault.Identity.Service.Application.Dtos;

namespace PensionVault.Identity.Service.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}
