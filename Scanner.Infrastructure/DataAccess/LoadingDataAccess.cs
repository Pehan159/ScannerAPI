using Scanner.Core.Domain.Entities.Loading;
using Scanner.Infrastructure.Data;
using System;
using System.Collections.Generic;

namespace Scanner.Infrastructure.DataAccess
{
    public class LoadingDataAccess
    {
        private readonly DataContextDapper _dapper;

        public LoadingDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        public IEnumerable<Weight> GetTotalLoadedWeight(string driverName)
        {
            string sql = @"
SELECT 
    SUM(rsd.""GlassWeight"") AS ""TotalWeightSum""
FROM 
    ""spilRunnSheetHeader"" sh
JOIN 
    ""spilRunnSheetDownloadedBarCodes"" r ON r.""RunnNo"" = sh.""RunnNo""
LEFT JOIN 
    ""spilRunnSheetDetailLines"" sdl ON sdl.""OrderIndex"" = r.""OrderIndex"" AND sdl.""iInvDetailID"" = r.""iInvDetailID""
LEFT JOIN 
    ""spilInvNumLines"" rsd ON rsd.""iInvDetailID"" = r.""iInvDetailID""
WHERE 
    sh.""DrivName"" LIKE @DriverName
    AND r.""BarcodeScannedStatus"" = 2
    AND rsd.""LineType"" = 3;
";
            return _dapper.LoadData<Weight>(sql, new { DriverName = $"%{driverName}%" });
        }

        public IEnumerable<RunningSheetHeaderDto> GetRunnAndLoadSheetByDriverName(string driverName)
        {
            string sql = @"
SELECT DISTINCT 
    rh.""LoadNo"", 
    rh.""RunnNo"", 
    rh.""VehRegNo"", 
    a.""Description"" AS ""AreaDescription"",  
    (
        SELECT COUNT(DISTINCT sn2.""cAccountName"") 
        FROM ""spilInvNum"" sn2 
        JOIN ""spilRunnSheetDetailLines"" sdl2 ON sdl2.""OrderIndex"" = sn2.""OrderIndex""
        WHERE sdl2.""RunnNo"" = rh.""RunnNo""
    ) AS ""TotalCustomerCount"",
    SUM(snl.""fQuantity"") AS ""TotalItemsToBeLoaded"", 
    SUM(snl.""GlassWeight"") AS ""TotalWeightToBeLoaded"",
    (
        SELECT ""MaxGlassWeight""
        FROM ""spilVehicleMaster""
        WHERE ""Name"" = rh.""VehRegNo""
    ) AS ""VehichleMaxGlassWeight""
FROM 
    ""spilRunnSheetHeader"" rh 
JOIN 
    ""spilRunnSheetDetailLines"" sdl ON rh.""RunnNo"" = sdl.""RunnNo""
JOIN 
    ""spilInvNum"" sn ON sdl.""OrderIndex"" = sn.""OrderIndex"" AND sn.""ProductionState"" = 8
JOIN 
    ""spilInvNumLines"" snl ON sn.""OrderIndex"" = snl.""OrderIndex"" AND sdl.""iInvDetailID"" = snl.""iInvDetailID""
JOIN 
    ""Areas"" a ON rh.""AreaID"" = a.""idAreas""
WHERE 
    rh.""DrivName"" LIKE @DriverName
    AND rh.""LoadNo"" > 0
    AND sn.""DelMethodID"" != 2 AND snl.""LineType"" = 3
GROUP BY 
    rh.""LoadNo"", rh.""RunnNo"", rh.""VehRegNo"", a.""Description"";
";
            return _dapper.LoadData<RunningSheetHeaderDto>(sql, new { DriverName = $"%{driverName}%" });
        }

        public IEnumerable<Barcodes> GetAllBarcodesForRunnSheet(int runnNo)
        {
            string sql = @"
SELECT DISTINCT ps.""BarCodeV"", ps.""iInvDetailID""
FROM ""spilPROD_SERIALS"" ps
JOIN ""spilInvNum"" sn ON ps.""OrderIndex"" = sn.""OrderIndex""
WHERE ps.""OrderIndex"" IN (
    SELECT rd.""OrderIndex""
    FROM ""spilRunnSheetDetailLines"" rd
    WHERE rd.""RunnNo"" = @RunnNo
)
AND ps.""BarCodeV"" LIKE 'L%';";

            return _dapper.LoadData<Barcodes>(sql, new { RunnNo = runnNo });
        }

        public IEnumerable<Barcodes> GetAllScannedBarcodesForRunnSheet(int runnNo)
        {
            string sql = @"
        SELECT DISTINCT ""SerialBarcodeValue"" AS ""BarCodeV""
        FROM ""spilRunnSheetDownloadedBarCodes""
        WHERE ""RunnNo"" = @RunnNo;";

            return _dapper.LoadData<Barcodes>(sql, new { RunnNo = runnNo });
        }


        public void SaveScanDetails(ScanDetailsRequest request)
        {
            int barcodeScannedStatus = 2;
            int userId = 89;

            try
            {
                foreach (string barcodeValue in request.BarCodeValue)
                {
                    string barcodeValueShortened = barcodeValue.Substring(0, barcodeValue.LastIndexOf('-'));

                    bool isBarcodeScanned = _dapper.ExecuteScalar<bool>(
                        @"SELECT EXISTS (SELECT 1 FROM ""spilRunnSheetDownloadedBarCodes"" WHERE ""BarcodeValue"" = @BarCodeValue);",
                        new { BarCodeValue = barcodeValue });

                    if (isBarcodeScanned)
                    {
                        string updateBarcodeSql = @"
UPDATE ""spilRunnSheetDownloadedBarCodes""
SET ""Status"" = @Status,
    ""ScannedDateTime"" = NOW()
WHERE ""BarcodeValue"" = @BarcodeValue;";

                        _dapper.Execute(updateBarcodeSql, new
                        {
                            Status = barcodeScannedStatus,
                            BarcodeValue = barcodeValue
                        });
                    }
                    else
                    {
                        int orderIndex = _dapper.ExecuteScalar<int>(
                            @"SELECT ""OrderIndex"" FROM ""spilPROD_SERIALS"" WHERE ""BarCodeV"" = @BarCodeValue;",
                            new { BarCodeValue = barcodeValue });

                        int iInvDetailID = _dapper.ExecuteScalar<int>(
                            @"SELECT ""iInvDetailID"" FROM ""spilPROD_SERIALS"" WHERE ""BarCodeV"" = @BarCodeValue;",
                            new { BarCodeValue = barcodeValue });

                        string insertBarcodeSql = @"
INSERT INTO ""spilRunnSheetDownloadedBarCodes"" 
    (""RunnNo"", ""BarcodeValue"", ""BarcodeTrolley"", ""TaggedTime"", ""Status"", ""Qty"", ""SerialBarcodeValue"", ""OrderIndex"", ""iInvDetailID"", ""BarcodeScannedStatus"", ""UserID"", ""ScannedDateTime"") 
VALUES 
    (@RunnNo, @BarcodeValue, '', NOW(), 'OK', 1, @SerialBarcodeValue, @OrderIndex, @iInvDetailID, @BarcodeScannedStatus, @UserID, NOW());";

                        _dapper.Execute(insertBarcodeSql, new
                        {
                            RunnNo = request.RunnNo,
                            BarcodeValue = barcodeValueShortened,
                            SerialBarcodeValue = barcodeValue,
                            OrderIndex = orderIndex,
                            iInvDetailID = iInvDetailID,
                            BarcodeScannedStatus = barcodeScannedStatus,
                            UserID = userId
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
            }
        }

        public IEnumerable<ScannedSummary> GetLoadedScannedSummary(int runnNo)
        {
            string sql = @"
SELECT 
    b.""RunnNo"", 
    sn.""ExtOrderNum"", 
    MIN(rsd.""Description"") AS ""ItemDescription"", 
    b.""BarcodeValue"", 
    b.""BarcodeScannedStatus"", 
    MIN(snl.""fQuantity"") AS ""fQuantity"", 
    MIN(snl.""iHeight"") AS ""iHeight"", 
    MIN(snl.""iWidth"") AS ""iWidth""
FROM ""spilRunnSheetDownloadedBarCodes"" b
INNER JOIN ""spilRunnSheetDetailLines"" rsd ON b.""OrderIndex"" = rsd.""OrderIndex""
INNER JOIN ""spilInvNum"" sn ON sn.""OrderIndex"" = b.""OrderIndex""
INNER JOIN ""spilInvNumLines"" snl ON sn.""OrderIndex"" = snl.""OrderIndex""
WHERE b.""BarcodeScannedStatus"" = 2 
  AND b.""RunnNo"" = @RunnNo
GROUP BY 
    b.""RunnNo"", 
    sn.""ExtOrderNum"", 
    b.""BarcodeValue"", 
    b.""BarcodeScannedStatus"";
";
            return _dapper.LoadData<ScannedSummary>(sql, new { RunnNo = runnNo });
        }
    }
}
