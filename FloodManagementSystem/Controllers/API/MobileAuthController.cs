using GlobalDisasterManagement.Data;
using GlobalDisasterManagement.Models;
using GlobalDisasterManagement.Models.DTO.Mobile;
using GlobalDisasterManagement.Services.Abstract;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlobalDisasterManagement.Controllers.API
{
    [Route("api/mobile/[controller]")]
    [ApiController]
    public class MobileAuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IJwtService _jwtService;
        private readonly DisasterDbContext _context;
        private readonly ILogger<MobileAuthController> _logger;

        public MobileAuthController(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IJwtService jwtService,
            DisasterDbContext context,
            ILogger<MobileAuthController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Mobile login with JWT token
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<MobileApiResponse<MobileAuthResponse>>> Login([FromBody] MobileLoginRequest request)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Invalid email or password"
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email!, user.UserName!, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));

                // Save device token if provided
                if (!string.IsNullOrEmpty(request.DeviceToken) && request.Platform.HasValue)
                {
                    await SaveDeviceTokenAsync(user.Id, request.DeviceToken, request.Platform.Value, request.DeviceInfo);
                }

                var authResponse = new MobileAuthResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new MobileUserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName!,
                        Email = user.Email!,
                        PhoneNumber = user.PhoneNumber,
                        CityName = user.CityName,
                        LGAName = user.LGAName,
                        EmailConfirmed = user.EmailConfirmed
                    }
                };

                _logger.LogInformation("Mobile login successful for user {Email}", request.Email);
                return Ok(new MobileApiResponse<MobileAuthResponse> { Success = true, Data = authResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during mobile login for {Email}", request.Email);
                return StatusCode(500, new MobileApiResponse<MobileAuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during login"
                });
            }
        }

        /// <summary>
        /// Mobile registration
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<MobileApiResponse<MobileAuthResponse>>> Register([FromBody] MobileRegisterRequest request)
        {
            try
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Email already registered"
                    });
                }

                var user = new User
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    PhoneNumber = request.PhoneNumber,
                    CityId = request.CityId ?? 0,
                    LGAId = request.LGAId ?? 0
                };

                var result = await _userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = string.Join(", ", result.Errors.Select(e => e.Description))
                    });
                }

                await _userManager.AddToRoleAsync(user, "User");

                // Generate tokens
                var roles = await _userManager.GetRolesAsync(user);
                var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Email, user.UserName, roles);
                var refreshToken = _jwtService.GenerateRefreshToken();

                await _jwtService.SaveRefreshTokenAsync(user.Id, refreshToken, DateTime.UtcNow.AddDays(30));

                // Save device token if provided
                if (!string.IsNullOrEmpty(request.DeviceToken) && request.Platform.HasValue)
                {
                    await SaveDeviceTokenAsync(user.Id, request.DeviceToken, request.Platform.Value, null);
                }

                var authResponse = new MobileAuthResponse
                {
                    Success = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60),
                    User = new MobileUserDto
                    {
                        Id = user.Id,
                        UserName = user.UserName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        EmailConfirmed = false
                    }
                };

                _logger.LogInformation("Mobile registration successful for {Email}", request.Email);
                return Ok(new MobileApiResponse<MobileAuthResponse> { Success = true, Data = authResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during mobile registration for {Email}", request.Email);
                return StatusCode(500, new MobileApiResponse<MobileAuthResponse>
                {
                    Success = false,
                    Message = "An error occurred during registration"
                });
            }
        }

        /// <summary>
        /// Refresh access token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<MobileApiResponse<MobileAuthResponse>>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(Request.Headers["Authorization"].ToString().Replace("Bearer ", ""));
                if (principal == null)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Invalid access token"
                    });
                }

                var userId = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Invalid token claims"
                    });
                }

                var isValid = await _jwtService.ValidateRefreshTokenAsync(userId, request.RefreshToken);
                if (!isValid)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    });
                }

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return Ok(new MobileApiResponse<MobileAuthResponse>
                    {
                        Success = false,
                        Message = "User not found"
                    });
                }

                var roles = await _userManager.GetRolesAsync(user);
                var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Email!, user.UserName!, roles);
                var newRefreshToken = _jwtService.GenerateRefreshToken();

                await _jwtService.SaveRefreshTokenAsync(user.Id, newRefreshToken, DateTime.UtcNow.AddDays(30));

                var authResponse = new MobileAuthResponse
                {
                    Success = true,
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(60)
                };

                return Ok(new MobileApiResponse<MobileAuthResponse> { Success = true, Data = authResponse });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return StatusCode(500, new MobileApiResponse<MobileAuthResponse>
                {
                    Success = false,
                    Message = "An error occurred while refreshing token"
                });
            }
        }

        /// <summary>
        /// Logout and revoke refresh token
        /// </summary>
        [Authorize]
        [HttpPost("logout")]
        public async Task<ActionResult<MobileApiResponse<object>>> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId != null)
                {
                    await _jwtService.RevokeRefreshTokenAsync(userId);
                    _logger.LogInformation("User {UserId} logged out", userId);
                }

                return Ok(new MobileApiResponse<object>
                {
                    Success = true,
                    Message = "Logged out successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred during logout"
                });
            }
        }

        /// <summary>
        /// Update device token for push notifications
        /// </summary>
        [Authorize]
        [HttpPost("device-token")]
        public async Task<ActionResult<MobileApiResponse<object>>> UpdateDeviceToken([FromBody] UpdateDeviceTokenRequest request)
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (userId == null)
                {
                    return Unauthorized();
                }

                await SaveDeviceTokenAsync(userId, request.DeviceToken, request.Platform, request.DeviceInfo);

                return Ok(new MobileApiResponse<object>
                {
                    Success = true,
                    Message = "Device token updated successfully"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating device token");
                return StatusCode(500, new MobileApiResponse<object>
                {
                    Success = false,
                    Message = "An error occurred while updating device token"
                });
            }
        }

        private async Task SaveDeviceTokenAsync(string userId, string token, DevicePlatform platform, string? deviceInfo)
        {
            var existingToken = await _context.DeviceTokens
                .FirstOrDefaultAsync(dt => dt.UserId == userId && dt.Token == token);

            if (existingToken != null)
            {
                existingToken.IsActive = true;
                existingToken.UpdatedAt = DateTime.UtcNow;
                existingToken.LastUsedAt = DateTime.UtcNow;
                if (!string.IsNullOrEmpty(deviceInfo))
                {
                    existingToken.DeviceInfo = deviceInfo;
                }
            }
            else
            {
                var deviceToken = new DeviceToken
                {
                    UserId = userId,
                    Token = token,
                    Platform = platform,
                    IsActive = true,
                    DeviceInfo = deviceInfo,
                    LastUsedAt = DateTime.UtcNow
                };
                _context.DeviceTokens.Add(deviceToken);
            }

            await _context.SaveChangesAsync();
        }
    }
}
