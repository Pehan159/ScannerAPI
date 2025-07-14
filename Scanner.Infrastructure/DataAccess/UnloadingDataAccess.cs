using Scanner.Core.Domain.Dto.Unloading;
using Scanner.Core.Domain.Entities.Loading;
using Scanner.Core.Domain.Entities.Unloading;
using Scanner.Infrastructure.Data;

namespace Scanner.Infrastructure.DataAccess
{
    public class UnloadingDataAccess
    {
        private readonly DataContextDapper _dapper;

        public UnloadingDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        public IEnumerable<Unloading> GetLoadedRunnSheet(string driverName)
        {
            string sql = @"
SELECT 
    sh.""RunnNo"", 
    sh.""LoadNo"",
    sh.""VehRegNo"",  
    a.""Description"" AS ""AreaDescription"",
    COUNT(DISTINCT inv.""cAccountName"") AS ""TotalCustomerCount"",
    COUNT(DISTINCT r.""SerialBarcodeValue"") AS ""TotalLoadedItems"",
    SUM(rsd.""GlassWeight"") AS ""TotalWeight""
FROM 
    ""spilRunnSheetHeader"" sh
JOIN 
    ""spilRunnSheetDownloadedBarCodes"" r ON r.""RunnNo"" = sh.""RunnNo""
JOIN 
    ""Areas"" a ON sh.""AreaID"" = a.""idAreas""
LEFT JOIN 
    ""spilInvNum"" inv ON inv.""OrderIndex"" = r.""OrderIndex""
LEFT JOIN 
    ""spilRunnSheetDetailLines"" sdl ON sdl.""OrderIndex"" = r.""OrderIndex""
LEFT JOIN 
    ""spilInvNumLines"" rsd ON rsd.""iInvDetailID"" = r.""iInvDetailID""
WHERE 
    sh.""DrivName"" ILIKE @DriverName
    AND r.""BarcodeScannedStatus"" = 2 
    AND rsd.""LineType"" = 3
GROUP BY 
    sh.""RunnNo"", sh.""LoadNo"", sh.""VehRegNo"", a.""Description"";
";

            return _dapper.LoadData<Unloading>(sql, new { DriverName = $"%{driverName}%" });
        }

        public IEnumerable<UnloadingCustomersDto> GetCustomersByRunnNo(int runnNo)
        {
            string sql = @"
SELECT
    inv.""cAccountName"" AS ""Customer"",
    CONCAT_WS(', ', inv.""Address1"", inv.""Address2"", inv.""Address3"", inv.""Address4"", inv.""Address5"") AS ""Address"",
    COUNT(DISTINCT r.""SerialBarcodeValue"") AS ""TotalItemsLoaded""
FROM 
    ""spilRunnSheetHeader"" sh
JOIN 
    ""spilRunnSheetDownloadedBarCodes"" r ON r.""RunnNo"" = sh.""RunnNo""
JOIN 
    ""Areas"" a ON sh.""AreaID"" = a.""idAreas""
LEFT JOIN 
    ""spilInvNum"" inv ON inv.""OrderIndex"" = r.""OrderIndex""
LEFT JOIN 
    ""spilRunnSheetDetailLines"" sdl ON sdl.""OrderIndex"" = r.""OrderIndex""
LEFT JOIN 
    ""spilInvNumLines"" rsd ON rsd.""iInvDetailID"" = r.""iInvDetailID""
WHERE 
    sh.""RunnNo"" = @RunnNo
    AND r.""BarcodeScannedStatus"" = 2 
    AND rsd.""LineType"" = 3
GROUP BY 
    inv.""cAccountName"",
    inv.""Address1"", 
    inv.""Address2"", 
    inv.""Address3"", 
    inv.""Address4"", 
    inv.""Address5"";
";

            return _dapper.LoadData<UnloadingCustomersDto>(sql, new { RunnNo = runnNo });
        }

        public IEnumerable<Barcodes> GetBarcodesforCustomer(string customerName, int runnNo)
        {
            string sql = @"
SELECT DISTINCT r.""SerialBarcodeValue"" AS ""BarCodeV""
FROM ""spilRunnSheetDownloadedBarCodes"" r
JOIN ""spilRunnSheetHeader"" sh ON r.""RunnNo"" = sh.""RunnNo""
JOIN ""spilInvNum"" inv ON r.""OrderIndex"" = inv.""OrderIndex""
WHERE sh.""RunnNo"" = @RunnNo
  AND inv.""cAccountName"" = @CustomerName 
  AND r.""BarcodeScannedStatus"" = 2;
";

            return _dapper.LoadData<Barcodes>(sql, new { CustomerName = customerName, RunnNo = runnNo });
        }

        public void ClickForUnloading(string customerName, int runnNo, string signatureUrl, string imageurl)
        {
            string updateStatusSql = @"
UPDATE ""spilRunnSheetDownloadedBarCodes"" r
SET ""BarcodeScannedStatus"" = 3
FROM ""spilRunnSheetHeader"" sh, ""spilInvNum"" inv
WHERE r.""RunnNo"" = sh.""RunnNo""
  AND r.""OrderIndex"" = inv.""OrderIndex""
  AND sh.""RunnNo"" = @RunnNo
  AND inv.""cAccountName"" = @CustomerName;
";
            _dapper.Execute(updateStatusSql, new { CustomerName = customerName, RunnNo = runnNo });
        }

        public void OneTimeScanForUnloading(string customerName, int runnNo, string barcode)
        {
            string updateStatusSql = @"
UPDATE ""spilRunnSheetDownloadedBarCodes"" r
SET ""BarcodeScannedStatus"" = 3
FROM ""spilRunnSheetHeader"" sh, ""spilInvNum"" inv
WHERE r.""RunnNo"" = sh.""RunnNo""
  AND r.""OrderIndex"" = inv.""OrderIndex""
  AND sh.""RunnNo"" = @RunnNo
  AND inv.""cAccountName"" = @CustomerName;
";

            _dapper.Execute(updateStatusSql, new { CustomerName = customerName, RunnNo = runnNo });
        }

        public async Task<int> UpdateUnloadingAsync(UnloadingRequest req)
        {
            string sql = @"
        UPDATE ""spilRunnSheetDownloadedBarCodes"" r
        SET ""BarcodeScannedStatus"" = 3
        FROM ""spilRunnSheetHeader"" sh, ""spilInvNum"" inv
        WHERE r.""RunnNo"" = sh.""RunnNo""
          AND r.""OrderIndex"" = inv.""OrderIndex""
          AND sh.""RunnNo"" = @RunnNo
          AND inv.""cAccountName"" = @CustomerName
          AND r.""SerialBarcodeValue"" = ANY(@Barcodes);
    ";

            var parameters = new
            {
                req.CustomerName,
                req.RunnNo,
                Barcodes = req.Barcode // string[] or List<string>
            };

            // Debug print
            Console.WriteLine("Executing UpdateUnloadingAsync with parameters:");
            Console.WriteLine($"CustomerName: {req.CustomerName}");
            Console.WriteLine($"RunnNo: {req.RunnNo}");
            Console.WriteLine("Barcodes: " + string.Join(", ", req.Barcode));

            int rowsAffected = await Task.Run(() => _dapper.Execute(sql, parameters));
            return rowsAffected;
        }


        public IEnumerable<ScannedSummary> GetUnloadedScannedSummary(int runnNo, string customerName)
        {
            string sql = @"
SELECT 
    sh.""RunnNo"", 
    rsd.""ExtOrderNum"", 
    MIN(rr.""Description"") AS ""ItemDescription"", 
    r.""BarcodeValue"", 
    MIN(r.""iInvDetailID"") AS ""iInvDetailID"", 
    r.""BarcodeScannedStatus""
FROM ""spilRunnSheetDownloadedBarCodes"" r
JOIN ""spilRunnSheetHeader"" sh ON r.""RunnNo"" = sh.""RunnNo""
JOIN ""spilInvNum"" rsd ON r.""OrderIndex"" = rsd.""OrderIndex""
JOIN ""spilRunnSheetDetailLines"" rr ON rr.""OrderIndex"" = rsd.""OrderIndex""
WHERE r.""BarcodeScannedStatus"" IN (3, 4) 
  AND sh.""RunnNo"" = @RunnNo 
  AND rsd.""cAccountName"" = @CustomerName
GROUP BY sh.""RunnNo"", rsd.""ExtOrderNum"", r.""BarcodeValue"", r.""BarcodeScannedStatus"";
";

            return _dapper.LoadData<ScannedSummary>(sql, new { RunnNo = runnNo, CustomerName = customerName });
        }
    }
}
