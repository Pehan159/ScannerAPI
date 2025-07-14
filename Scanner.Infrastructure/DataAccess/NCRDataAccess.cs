using Microsoft.AspNetCore.Mvc;
using Scanner.Core.Domain.Entities.NCR;
using Scanner.Infrastructure.Data;

namespace Scanner.Infrastructure.DataAccess
{
    public class NCRDataAccess
    {
        private readonly DataContextDapper _dapper;

        public NCRDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        public IActionResult ProcessNCR(NcrRequest request)
        {
            if (string.IsNullOrEmpty(request.barcodeValue))
            {
                return new BadRequestObjectResult("Barcode value cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(request.reason))
            {
                return new BadRequestObjectResult("Add a reason.");
            }

            string existingBarcodeQuery = @"SELECT COUNT(*) 
                                            FROM ""NCRRequestDetails"" 
                                            WHERE ""iBarcodeValue"" = @BarcodeValue";
            var existingBarcodeParams = new { BarcodeValue = request.barcodeValue };
            var existingBarcodeCount = _dapper.QueryFirstOrDefault<int>(existingBarcodeQuery, existingBarcodeParams);
            if (existingBarcodeCount > 0)
            {
                return new BadRequestObjectResult("Barcode has already been scanned previously.");
            }

            string barcodeQuery = @"SELECT ""OrderIndex"", ""iInvDetailID"" 
                                    FROM ""spilPROD_SERIALS"" 
                                    WHERE ""BarCodeV"" = @BarcodeValue";
            var barcodeParams = new { BarcodeValue = request.barcodeValue };
            var barcodeResult = _dapper.QueryFirstOrDefault<dynamic>(barcodeQuery, barcodeParams);

            if (barcodeResult == null)
            {
                return new NotFoundObjectResult("Barcode value not found.");
            }

            int orderIndex = barcodeResult.OrderIndex;
            int iInvDetailID = barcodeResult.iInvDetailID;

            string orderDetailsQuery = @"SELECT ""OrderNum"", ""OrderIndex"", ""ExtOrderNum"", ""cAccountName"" AS ""CustomerName"" 
                                         FROM ""spilInvNum"" 
                                         WHERE ""OrderIndex"" = @OrderIndex";
            var orderDetailsParams = new { OrderIndex = orderIndex };
            var orderDetailsResult = _dapper.QueryFirstOrDefault<dynamic>(orderDetailsQuery, orderDetailsParams);

            if (orderDetailsResult == null)
            {
                return new NotFoundObjectResult("Order details not found.");
            }

            string orderNum = orderDetailsResult.OrderNum;
            string extOrderNum = orderDetailsResult.ExtOrderNum;
            string customerName = orderDetailsResult.CustomerName;

            string runnSheetQuery = @"SELECT ""RunnNo"" 
                                      FROM ""spilRunnSheetDetailLines"" 
                                      WHERE ""OrderIndex"" = @OrderIndex AND ""iInvDetailID"" = @iInvDetailID";
            var runnSheetParams = new { OrderIndex = orderIndex, iInvDetailID };
            var runnSheetResult = _dapper.QueryFirstOrDefault<int>(runnSheetQuery, runnSheetParams);

            if (runnSheetResult == 0)
            {
                return new NotFoundObjectResult("Runn sheet details not found.");
            }

            int runnNo = runnSheetResult;

            string driverIDQuery = @"SELECT s.""ID"" 
                                     FROM ""spilDriverMaster"" s 
                                     JOIN ""spilRunnSheetHeader"" ss ON ss.""RunnNo"" = @RunnNo AND ss.""DrivName"" = s.""Name""";
            var driverIDParams = new { RunnNo = runnNo };
            var driverIDResult = _dapper.QueryFirstOrDefault<int?>(driverIDQuery, driverIDParams);

            if (driverIDResult == null)
            {
                return new NotFoundObjectResult("Driver details not found.");
            }

            int driverID = driverIDResult.Value;

            string insertQuery = @"INSERT INTO ""NCRRequestDetails"" 
            (""OrderIndex"", ""iInvDetailID"", ""RunnNo"", ""DriverID"", ""NcrReason"", ""NcrRequestedDate"", ""iBarcodeValue"") 
            VALUES (@OrderIndex, @iInvDetailID, @RunnNo, @DriverID, @NcrReason, @NcrRequestedDate, @iBarcodeValue)";
            _dapper.Execute(insertQuery, new
            {
                OrderIndex = orderIndex,
                iInvDetailID,
                RunnNo = runnNo,
                DriverID = driverID,
                NcrReason = request.reason,
                NcrRequestedDate = DateTime.Now,
                iBarcodeValue = request.barcodeValue
            });

            string updateBarcodeStatusQuery = @"UPDATE ""spilRunnSheetDownloadedBarCodes"" 
                                                SET ""BarcodeScannedStatus"" = 4 
                                                WHERE ""SerialBarcodeValue"" = @SerialBarCodeValue";
            _dapper.Execute(updateBarcodeStatusQuery, new { SerialBarCodeValue = request.barcodeValue });

            return new OkObjectResult("NCR requested successfully");
        }
    }
}
