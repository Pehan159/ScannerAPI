using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Application.Interfaces;
using Scanner.Core.Entities;

namespace Scanner.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;

        public TenantController(ITenantService tenantService)
        {
            _tenantService = tenantService;
        }

        [HttpPost("authenticate")]
        public IActionResult Authenticate([FromBody] LoginRequest loginRequest)
        {
            var tenantGuid = _tenantService.AuthenticateTenantUser(loginRequest.Username, loginRequest.Password, loginRequest.DeviceId);
            if (tenantGuid == null)
            {
                return Unauthorized("Invalid credentials");
            }

            return Ok(new { TenantGuid = tenantGuid });
        }
    }


}
