using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GlobalDisasterManagement.Services.Abstract
{
    public interface IJwtService
    {
        string GenerateAccessToken(string userId, string email, string userName, IList<string> roles);
        string GenerateRefreshToken();
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt);
        Task<string?> GetRefreshTokenAsync(string userId);
        Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
        Task RevokeRefreshTokenAsync(string userId);
    }
}
