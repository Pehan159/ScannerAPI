using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Domain.Entities.NCR;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class NCRController : ControllerBase
    {
        private readonly NCRDataAccess _dataAccess;

        public NCRController(NCRDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpPost("NCRPost")]
        public IActionResult NCRPost([FromBody] NcrRequest request)
        {
            try
            {
                return _dataAccess.ProcessNCR(request);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
