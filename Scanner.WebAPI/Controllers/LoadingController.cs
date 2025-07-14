using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Domain.Entities.Loading;
using Scanner.Infrastructure.DataAccess;

namespace Scanner.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LoadingController : ControllerBase
    {
        private readonly LoadingDataAccess _dataAccess;

        public LoadingController(LoadingDataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        [HttpGet("GetTotalLoadedWeight/{driverName}")]
        public IActionResult GetTotalLoadedWeight(string driverName)
        {
            try
            {
                var runningSheetDetails = _dataAccess.GetTotalLoadedWeight(driverName);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No items have been loaded.");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }





        [HttpGet("GetRunnAndLoadSheetByDriverName/{driverName}")]
        public IActionResult GetRunnAndLoadSheetByDriverName(string driverName)
        {
            try
            {
                IEnumerable<RunningSheetHeaderDto> runningSheetDetails = _dataAccess.GetRunnAndLoadSheetByDriverName(driverName);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No running sheet headers found for the specified driver.");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("GetAllBarcodesForRunnSheet/{runnNo}")]
        public IActionResult GetAllBarcodesForRunnSheet(int runnNo)
        {
            try
            {
                IEnumerable<Barcodes> runningSheetDetails = _dataAccess.GetAllBarcodesForRunnSheet(runnNo);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No piece barcodes for running sheet");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("getAllScannedBarcodesForRunnSheet/{runnNo}")]
        public IActionResult GetAllScannedBarcodesForRunnSheet(int runnNo)
        {
            try
            {
                IEnumerable<Barcodes> runningSheetDetails = _dataAccess.GetAllScannedBarcodesForRunnSheet(runnNo);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No scanned barcodes for running sheet");
                }

                return Ok(runningSheetDetails);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("SaveScanDetails")]
        public IActionResult SaveScanDetails([FromBody] ScanDetailsRequest request)
        {
            try
            {
                _dataAccess.SaveScanDetails(request);
                return Ok("Scan details saved successfully.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
  

        [HttpGet("GetLoadedScannedSummary/{runnNo}")]
        public IActionResult GetLoadedScannedSummary(int runnNo)
        {
            try
            {
                IEnumerable<ScannedSummary> runningSheetDetails = _dataAccess.GetLoadedScannedSummary(runnNo);

                if (runningSheetDetails == null || !runningSheetDetails.Any())
                {
                    return NotFound("No scan summary for the specified running sheet.");
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

