using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Domain.Entities.ConfirmDelivery;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]

    public class ConfirmDeliveryController : ControllerBase
    {
        private readonly ConfirmDeliveryDataAccess _repository;

        public ConfirmDeliveryController(ConfirmDeliveryDataAccess repository)
        {
            _repository = repository;
        }

        [HttpPost("ConfirmDelivery")]
        public async Task<IActionResult> ConfirmDelivery([FromForm] SignatureUploadModel request)
        {
            if (request.SignatureFileURL == null || request.ImageFileURL == null)
            {
                return NotFound("Please upload a signature or image");
            }

            int driverId = _repository.GetDriverId(request.RunnNo);
            if (driverId == 0)
            {
                return NotFound("Driver details not found.");
            }

            string customerName = request.CustomerName;

            _repository.InsertIntoHeader(request.RunnNo, customerName, driverId, request.SignatureFileURL, request.ImageFileURL);

            int despatchId = _repository.GetMaxDespatchID();

            _repository.InsertIntoDetailLines(request.RunnNo, despatchId, customerName);

            return Ok("Delivery confirmed successfully.");
        }
    }
}
