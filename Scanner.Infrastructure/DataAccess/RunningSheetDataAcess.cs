using Scanner.Core.Domain.Entities.RunningSheet;
using Scanner.Infrastructure.Data;

namespace Scanner.Infrastructure.DataAccess
{
    public class RunningSheetDataAccess
    {
        private readonly DataContextDapper _dapper;

        public RunningSheetDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        public IEnumerable<RunningSheetCustomersDto> GetCustomersForDriverByRunnNo(int runnNo)
        {
            string sql = @"
SELECT 
    ""Customer"",
    ""Address"",
    COUNT(DISTINCT ""OrderIndex"") AS ""TotalOrders"",
    SUM(""GlassWeight"") AS ""TotalWeight"",
    SUM(""fQuantity"") AS ""TotalQuantity""
FROM
(
    SELECT 
        sn.""cAccountName"" AS ""Customer"", 
        rd.""OrderIndex"",
        COALESCE(sn.""Address1"", '') || ' ' || COALESCE(sn.""Address2"", '') || ' ' || COALESCE(sn.""Address3"", '') || ' ' || COALESCE(sn.""Address4"", '') || ' ' || COALESCE(sn.""Address5"", '') || ' ' || COALESCE(sn.""Address6"", '') AS ""Address"",
        snl.""GlassWeight"",
        snl.""fQuantity""
    FROM 
        ""spilRunnSheetDetail"" rd
    JOIN 
        ""spilInvNum"" sn ON rd.""OrderIndex"" = sn.""OrderIndex""
    JOIN 
        ""spilInvNumLines"" snl ON sn.""OrderIndex"" = snl.""OrderIndex""
    WHERE 
        rd.""RunnNo"" = @RunnNo AND snl.""LineType"" = 3
) AS sub
GROUP BY ""Customer"", ""Address""
";
            return _dapper.LoadData<RunningSheetCustomersDto>(sql, new { RunnNo = runnNo });
        }

        public IEnumerable<RunningSheetDetailDto> GetOrdersByRunnNoAndCustomer(int runnNo, string customerName)
        {
            string sql = @"
SELECT 
    rd.""RunnNo"", 
    sn.""OrderIndex"",
    sn.""ExtOrderNum"" AS ""CustomerOrderNum"", 
    CAST(sn.""DueDate"" AS DATE) AS ""DueDate"", 
    CONCAT_WS(', ', sn.""Address1"", sn.""Address2"", sn.""Address3"", sn.""Address4"", sn.""Address5"") AS ""Address"",
    sn.""cAccountName"" AS ""Customer"", 
    sn.""ContactName"", 
    sn.""ContactTelephone"", 
    (
        SELECT SUM(rsd.""fQuantity"") 
        FROM ""spilInvNumLines"" rsd 
        JOIN ""spilRunnSheetDetailLines"" rd2 ON rsd.""iInvDetailID"" = rd2.""iInvDetailID"" 
        WHERE rsd.""OrderIndex"" = sn.""OrderIndex"" 
        AND rd2.""RunnNo"" = @RunnNo AND rsd.""LineType"" = 3
    ) AS ""TotalGlassPanels"" 
FROM 
    ""spilRunnSheetDetail"" rd
JOIN 
    ""spilInvNum"" sn ON rd.""OrderIndex"" = sn.""OrderIndex"" 
JOIN 
    ""spilInvNumLines"" snl ON sn.""OrderIndex"" = snl.""OrderIndex"" 
WHERE  
    rd.""RunnNo"" =  @RunnNo
    AND sn.""cAccountName"" = @CustomerName AND snl.""LineType"" = 3
GROUP BY 
    rd.""RunnNo"", 
    sn.""OrderIndex"", 
    sn.""ExtOrderNum"", 
    sn.""DueDate"", 
    sn.""DeliveryDate"", 
    sn.""Address1"", 
    sn.""Address2"", 
    sn.""Address3"", 
    sn.""Address4"", 
    sn.""Address5"", 
    sn.""cAccountName"", 
    sn.""ContactName"", 
    sn.""ContactTelephone"";
";
            return _dapper.LoadData<RunningSheetDetailDto>(sql, new { RunnNo = runnNo, CustomerName = customerName });
        }

        public IEnumerable<ItemsPerOrder> GetItemDetailsByOrderIndex(int runnNo, int orderIndex)
        {
            string sql = @"
SELECT 
    DISTINCT rsd.""iInvDetailID"", 
    rsd.""cDescription"" AS ""Description"",
    rsd.""fQuantity"",
    rd.""OrderIndex"", 
    rd.""RunnNo"",
    CASE 
        WHEN sdl.""iInvDetailID"" IS NOT NULL THEN 1 
        ELSE 0 
    END AS ""ScannedStatus""
FROM 
    ""spilInvNumLines"" rsd 
JOIN 
    ""spilRunnSheetDetailLines"" rd ON rsd.""iInvDetailID"" = rd.""iInvDetailID""
LEFT JOIN 
    ""spilRunnSheetDownloadedBarCodes"" sdl ON rd.""iInvDetailID"" = sdl.""iInvDetailID""
WHERE 
    rsd.""OrderIndex"" = @OrderIndex
    AND rd.""RunnNo"" = @RunnNo
    AND rsd.""LineType"" = 3;
";
            return _dapper.LoadData<ItemsPerOrder>(sql, new { OrderIndex = orderIndex, RunnNo = runnNo });
        }

        public dynamic GetItemDetailsByBarcode(string barcode)
        {
            string sql = @"
SELECT 
    ps.""BarCodeV"", 
    ps.""iInvDetailID"", 
    rsd.""OrderIndex"", 
    rsd.""ThisDelQty"", 
    rsd.""Description"", 
    CONCAT(rsd.""Height"", ' x ', rsd.""Width"", ' x ', rsd.""Thickness"") AS ""Dimensions"", 
    rsd.""GlassWeight"", 
    h.""Status""
FROM 
    ""spilPROD_SERIALS"" ps
JOIN 
    ""spilRunnSheetDetailLines"" rsd ON ps.""iInvDetailID"" = rsd.""iInvDetailID""
JOIN 
    ""spilRunnSheetHeader"" h ON rsd.""RunnNo"" = h.""RunnNo""
WHERE 
    ps.""BarCodeV"" = @Barcode;
";
            return _dapper.LoadData<dynamic>(sql, new { Barcode = barcode }).FirstOrDefault();
        }
    }
}
