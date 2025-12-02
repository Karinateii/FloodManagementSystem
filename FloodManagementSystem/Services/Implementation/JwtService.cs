using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GlobalDisasterManagement.Services.Implementation
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly DisasterDbContext _context;
        private readonly ILogger<JwtService> _logger;

        public JwtService(
            IConfiguration configuration,
            DisasterDbContext context,
            ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _context = context;
            _logger = logger;
        }

        public string GenerateAccessToken(string userId, string email, string userName, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Name, userName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString())
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var secretKey = _configuration["JwtSettings:SecretKey"] 
                ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(
                    int.Parse(_configuration["JwtSettings:AccessTokenExpirationMinutes"] ?? "60")),
                SigningCredentials = credentials,
                Issuer = _configuration["JwtSettings:Issuer"] ?? "GlobalDisasterManagement",
                Audience = _configuration["JwtSettings:Audience"] ?? "MobileApp"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secretKey = _configuration["JwtSettings:SecretKey"] 
                ?? "YourSuperSecretKeyThatIsAtLeast32CharactersLong!";
            
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false // Don't validate expiration for expired tokens
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<bool> SaveRefreshTokenAsync(string userId, string refreshToken, DateTime expiresAt)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user == null) return false;

                // Store refresh token in user's SecurityStamp or create a separate table
                // For simplicity, using SecurityStamp (in production, use a dedicated table)
                user.SecurityStamp = $"{refreshToken}|{expiresAt:O}";
                await _context.SaveChangesAsync();

                _logger.LogInformation("Refresh token saved for user {UserId}", userId);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving refresh token for user {UserId}", userId);
                return false;
            }
        }

        public async Task<string?> GetRefreshTokenAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user?.SecurityStamp == null) return null;

                var parts = user.SecurityStamp.Split('|');
                if (parts.Length != 2) return null;

                var expiresAt = DateTime.Parse(parts[1]);
                if (expiresAt < DateTime.UtcNow)
                {
                    _logger.LogWarning("Refresh token expired for user {UserId}", userId);
                    return null;
                }

                return parts[0];
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving refresh token for user {UserId}", userId);
                return null;
            }
        }

        public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
        {
            var storedToken = await GetRefreshTokenAsync(userId);
            return storedToken != null && storedToken == refreshToken;
        }

        public async Task RevokeRefreshTokenAsync(string userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);
                if (user != null)
                {
                    user.SecurityStamp = null;
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Refresh token revoked for user {UserId}", userId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking refresh token for user {UserId}", userId);
            }
        }
    }
}
