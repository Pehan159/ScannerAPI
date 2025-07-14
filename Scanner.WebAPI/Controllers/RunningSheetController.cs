using GoogleMaps.LocationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class RunningSheetController : ControllerBase
    {
        private readonly RunningSheetDataAccess _runningSheetService;
        private readonly IConfiguration _configuration;

        public RunningSheetController(RunningSheetDataAccess runningSheetService, IConfiguration configuration)
        {
            _runningSheetService = runningSheetService;
            _configuration = configuration;
        }

        [HttpGet("GetCustomersForDriverByRunnNo/{runnNo}")]
        public IActionResult GetCustomersForDriverByRunnNo(int runnNo)
        {
            try
            {
                var customers = _runningSheetService.GetCustomersForDriverByRunnNo(runnNo);
                if (customers == null || !customers.Any())
                {
                    return NotFound("No customers found for the specified RunnNo.");
                }

                var apiKey = _configuration["GoogleMaps:ApiKey"];
                var gls = new GoogleLocationService(apikey: apiKey);

                foreach (var detail in customers)
                {
                    var fullAddress = detail.Address;
                    try
                    {
                        var latlong = gls.GetLatLongFromAddress(fullAddress);
                        detail.Latitude = latlong.Latitude;
                        detail.Longitude = latlong.Longitude;
                    }
                    catch (Exception)
                    {
                        detail.Latitude = 0;
                        detail.Longitude = 0;
                    }
                }

                return Ok(customers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetOrdersByRunnNoAndCustomer/{runnNo}/{customerName}")]
        public IActionResult GetOrdersByRunnNoAndCustomer(int runnNo, string customerName)
        {
            try
            {
                var orders = _runningSheetService.GetOrdersByRunnNoAndCustomer(runnNo, customerName);
                if (orders == null || !orders.Any())
                {
                    return NotFound("No orders found for the specified customer in the specified running sheet.");
                }

                var apiKey = _configuration["GoogleMaps:ApiKey"];
                var gls = new GoogleLocationService(apikey: apiKey);

                foreach (var detail in orders)
                {
                    var fullAddress = detail.Address;
                    try
                    {
                        var latlong = gls.GetLatLongFromAddress(fullAddress);
                        detail.Latitude = latlong.Latitude;
                        detail.Longitude = latlong.Longitude;
                    }
                    catch (Exception)
                    {
                        detail.Latitude = 0;
                        detail.Longitude = 0;
                    }
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetItemDetailsByOrderIndex/{runnNo}/{orderIndex}")]
        public IActionResult GetItemDetailsByOrderIndex(int runnNo, int orderIndex)
        {
            try
            {
                var items = _runningSheetService.GetItemDetailsByOrderIndex(runnNo, orderIndex);
                if (items == null || !items.Any())
                {
                    return NotFound("No items found for the specific order");
                }
                return Ok(items);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetItemDetailsByBarcode/{barcode}")]
        public IActionResult GetItemDetailsByBarcode(string barcode)
        {
            try
            {
                var result = _runningSheetService.GetItemDetailsByBarcode(barcode);
                if (result == null)
                {
                    return NotFound("No running sheet details found for the specified barcode.");
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
