using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Application.Interfaces;
using Scanner.Core.Dto;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DriversController : ControllerBase
    {
        private readonly IDriversService _driversService;

        public DriversController(IDriversService driversService)
        {
            _driversService = driversService;
        }

        [HttpPost("GetAllDrivers")]
        public IActionResult GetAllDrivers([FromBody] AuthenticationRequestModel model)
        {
            try
            {
                var driverDetails = _driversService.GetDriversList(model.TenentGuid, model.DeviceId, model.SecretKey);

                if (driverDetails == null || !driverDetails.Any())
                {
                    return NotFound("No running sheet headers found for the specified driver.");
                }

                return Ok(driverDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}