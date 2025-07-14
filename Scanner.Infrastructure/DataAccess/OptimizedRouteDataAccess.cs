using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Scanner.Infrastructure.Data;

namespace Scanner.Infrastructure.DataAccess
{
    public class OptimizedRouteDataAccess
    {
        private readonly DataContextDapper _dapper;
        private readonly ILogger<OptimizedRouteDataAccess> _logger;

        public OptimizedRouteDataAccess(DataContextDapper dataContext, ILogger<OptimizedRouteDataAccess> logger)
        {
            _dapper = dataContext;
            _logger = logger;
        }

        public IActionResult GetOptimizedRouteForRunnNo(string runnNo)
        {
            try
            {
                // Step 1: Get OrderIndexes for items where BarcodeScannedStatus = 2
                string sqlStep1 = @"
                    SELECT DISTINCT OrderIndex
                    FROM spilRunnSheetDownloadedBarCodes
                    WHERE RunnNo = @RunnNo AND BarcodeScannedStatus = 2";

                var orderIndexes = _dapper.LoadData<int>(sqlStep1, new { RunnNo = runnNo });

                if (orderIndexes == null || !orderIndexes.Any())
                {
                    return new NotFoundObjectResult("No order indexes found for the specified RunnNo.");
                }

                // Step 2: Get load sequence in ascending order for the list of OrderIndexes
                string sqlStep2 = @"
                    SELECT LoadSequence
                    FROM spilLoadListLines
                    WHERE OrderIndex IN @OrderIndexes
                    ORDER BY LoadSequence ASC";

                var loadSequences = _dapper.LoadData<int>(sqlStep2, new { OrderIndexes = orderIndexes });

                if (loadSequences == null || !loadSequences.Any())
                {
                    return new NotFoundObjectResult("No load sequences found for the specified OrderIndexes.");
                }

                // Step 3: Get addresses for the load sequences in order
                int minLoadSequence = loadSequences.Min();
                int maxLoadSequence = loadSequences.Max();

                string sqlGetAddress = @"
                    SELECT DISTINCT 
                        CONCAT_WS(', ', Address1, Address2, Address3, Address4, 'Australia') AS Address,
                        spilLoadListLines.LoadSequence
                    FROM 
                        spilInvNum
                    INNER JOIN 
                        spilLoadListLines ON spilInvNum.OrderIndex = spilLoadListLines.OrderIndex
                    WHERE 
                        spilLoadListLines.OrderIndex IN @OrderIndexes
                    ORDER BY 
                        spilLoadListLines.LoadSequence ASC";

                var addresses = _dapper.LoadData<(string Address, int LoadSequence)>(sqlGetAddress, new { OrderIndexes = orderIndexes });

                if (addresses == null || !addresses.Any())
                {
                    return new NotFoundObjectResult("No addresses found for the specified OrderIndexes.");
                }

                // Group the addresses by their values and take the first occurrence to avoid duplicates
                var groupedAddresses = addresses.GroupBy(a => a.Address)
                                                .Select(g => g.First())
                                                .OrderBy(a => a.LoadSequence)
                                                .ToList();

                // Extracting the complete route addresses
                var routeAddresses = groupedAddresses.Select(a => a.Address).ToList();

                // Construct the response object
                var route = new
                {
                    MinLoadSequenceNum = minLoadSequence,
                    MaxLoadSequenceNum = maxLoadSequence,
                    RouteAddresses = routeAddresses
                };

                return new OkObjectResult(route);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the request.");
                return new StatusCodeResult(500);
            }
        }
    }
}
