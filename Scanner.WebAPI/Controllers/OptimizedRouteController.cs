using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Scanner.Infrastructure.Data;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OptimizedRouteController : ControllerBase
    {
        private readonly OptimizedRouteDataAccess _optimizedRouteDataAccess;

        public OptimizedRouteController(DataContextDapper dataContext, ILogger<OptimizedRouteDataAccess> logger)
        {
            _optimizedRouteDataAccess = new OptimizedRouteDataAccess(dataContext, logger);
        }

        [HttpGet("GetOptimizedRouteForRunnNo/{runnNo}")]
        public IActionResult GetOptimizedRouteForRunnNo(string runnNo)
        {
            return _optimizedRouteDataAccess.GetOptimizedRouteForRunnNo(runnNo);
        }
    }
}
