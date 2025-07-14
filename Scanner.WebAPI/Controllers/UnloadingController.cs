using GoogleMaps.LocationServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Domain.Dto.Unloading;
using Scanner.Core.Domain.Entities.Loading;
using Scanner.Core.Domain.Entities.Unloading;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UnloadingController : ControllerBase
    {
        private readonly UnloadingDataAccess _dataAccess;
        private readonly IConfiguration _configuration;

        public UnloadingController(UnloadingDataAccess dataAccess, IConfiguration configuration)
        {
            _dataAccess = dataAccess;
            _configuration = configuration;
        }

        [HttpGet("GetLoadedRunnSheet/{driverName}")]
        public IActionResult GetLoadedRunnSheet(string driverName)
        {
            try
            {
                IEnumerable<Unloading> runningSheetDetails = _dataAccess.GetLoadedRunnSheet(driverName);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No running sheets to be unloaded");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetCustomersByRunnNo/{runnNo}")]
        public IActionResult GetCustomersByRunnNo(int runnNo)
        {
            try
            {
                IEnumerable<UnloadingCustomersDto> runningSheetCustomersDto = _dataAccess.GetCustomersByRunnNo(runnNo);

                if (runningSheetCustomersDto == null || !runningSheetCustomersDto.Any())
                {
                    return NotFound("No customers found for the specified RunnNo.");
                }

                var apiKey = _configuration["GoogleMaps:ApiKey"];
                var gls = new GoogleLocationService(apikey: apiKey);

                foreach (var detail in runningSheetCustomersDto)
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

                return Ok(runningSheetCustomersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetBarcodesforCustomer/{customerName}/{runnNo}")]
        public IActionResult GetBarcodesforCustomer(string customerName, int runnNo)
        {
            try
            {
                IEnumerable<Barcodes> runningSheetCustomersDto = _dataAccess.GetBarcodesforCustomer(customerName, runnNo);

                if (runningSheetCustomersDto == null || !runningSheetCustomersDto.Any())
                {
                    return NotFound("No items loaded for this customer");
                }

                return Ok(runningSheetCustomersDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("OneTimeScanForUnloading/{customerName}/{runnNo}/{Barcode}")]
        public IActionResult OneTimeScanForUnloading( string customerName, int runnNo, string Barcode)
        {
            try
            {
                _dataAccess.OneTimeScanForUnloading(customerName, runnNo, Barcode);

                return Ok("Barcode status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }



    [HttpPut("OneTimeScanForUnloadingByBarcode")]
    public async Task<IActionResult> OneTimeScanForUnloading([FromBody] UnloadingRequest request)
    {
        if (request == null || request.Barcode == null || !request.Barcode.Any())
        {
            return BadRequest("No barcodes provided.");
        }

        // Call the data access method to update records for all barcodes
        int updatedRows = await _dataAccess.UpdateUnloadingAsync(request);
        return Ok(new { updatedRows });
    }



        [HttpPut("ClickForUnloading/{customerName}/{runnNo}/{signatureUrl}/{imageurl}")]
        public IActionResult ClickForUnloading(string customerName, int runnNo, string signatureUrl, string imageurl)
        {
            try
            {
                _dataAccess.ClickForUnloading(customerName, runnNo,signatureUrl,imageurl);

                return Ok("Barcode status updated successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetUnloadedScannedSummary/{runnNo}/{customerName}")]
        public IActionResult GetUnloadedScannedSummary(int runnNo, string customerName)
        {
            try
            {
                IEnumerable<ScannedSummary> runningSheetDetails = _dataAccess.GetUnloadedScannedSummary(runnNo, customerName);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No items scanned");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}


