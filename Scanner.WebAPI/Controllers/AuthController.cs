using Microsoft.AspNetCore.Mvc;
using Scanner.Infrastructure.DataAccess;
using Scanner.Core.Application.Interfaces;
using Scanner.Core.Domain.Dto;
using Scanner.Core.Domain.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly IDriversService _driversService;
        private readonly ITokenService _tokenService;
        private readonly RefreshTokenRepository _refreshTokenRepository;
        private readonly ITenantService _tenantService;

        public AuthController(
            IConfiguration configuration,
            ILogger<AuthController> logger,
            IDriversService driversService,
            ITokenService tokenService,
            RefreshTokenRepository refreshTokenRepository,
            ITenantService tenantService
        )
        {
            _configuration = configuration;
            _logger = logger;
            _driversService = driversService;
            _tokenService = tokenService;
            _refreshTokenRepository = refreshTokenRepository;
            _tenantService = tenantService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthenticationRequestExtendModel request)
        {
            try
            {
                var drivers = _driversService.GetDriversList(request.TenentGuid, request.DeviceId, request.SecretKey);

                if (drivers != null && drivers.Any())
                {
                    var validUsers = drivers.Select(driver => driver.DriverName);

                    if (validUsers.Contains(request.SelectedName))
                    {
                        var claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, request.SelectedName),
                            new Claim("Name", request.SelectedName),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                            new Claim("TenantGuid", request.TenentGuid),
                            new Claim("DriverName", request.SelectedName)
                        };

                        var jwtToken = _tokenService.GenerateAccessToken(claims);

                        var refreshToken = new RefreshToken
                        {
                            Token = _tokenService.GenerateRefreshToken(),
                            UserName = request.SelectedName,
                            Expiration = DateTime.UtcNow.AddDays(7)
                        };

                        var connectionString = _tenantService.GetConnectionString(request.TenentGuid);
                        bool isSaved = await _refreshTokenRepository.AddRefreshTokenAsync(connectionString, refreshToken);

                        if (!isSaved)
                        {
                            _logger.LogError("Failed to save the refresh token.");
                            return StatusCode(500, "Failed to save the refresh token.");
                        }

                        return Ok(new { AccessToken = jwtToken, RefreshToken = refreshToken.Token });
                    }
                    else
                    {
                        _logger.LogWarning("Invalid User: " + request.SelectedName);
                        return Unauthorized("Invalid user credentials.");
                    }
                }
                else
                {
                    _logger.LogWarning("No drivers found.");
                    return StatusCode(500, "No drivers found.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred: {ex.Message}");
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
