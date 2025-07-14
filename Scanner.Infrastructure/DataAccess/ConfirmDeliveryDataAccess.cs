using Dapper;
using Scanner.Infrastructure.Data;

namespace Scanner.Infrastructure.DataAccess
{
    public class ConfirmDeliveryDataAccess
    {
        private readonly DataContextDapper _dapper;

        public ConfirmDeliveryDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        public int GetDriverId(int runnNo)
        {
            string query = @"
                SELECT s.""ID""
                FROM ""spilDriverMaster"" s
                JOIN ""spilRunnSheetHeader"" ss ON ss.""RunnNo"" = @RunnNo AND ss.""DrivName"" = s.""Name""";

            return _dapper.QueryFirstOrDefault<int>(query, new { RunnNo = runnNo });
        }

        public string GetCustomerName(int runnNo)
        {
            string query = @"
                SELECT i.""cAccountName"" AS ""CustomerName""
                FROM ""spilInvNum"" i
                WHERE i.""OrderIndex"" = (
                    SELECT ""OrderIndex"" FROM ""spilRunnSheetHeader"" WHERE ""RunnNo"" = @RunnNo
                )";

            return _dapper.QueryFirstOrDefault<string>(query, new { RunnNo = runnNo });
        }

        public int GetMaxDespatchID()
        {
            string query = @"SELECT MAX(""DespatchID"") FROM ""MobileDespatchConfirmationHeader"";";
            return _dapper.QueryFirstOrDefault<int?>(query) ?? 0;
        }

        public void InsertIntoHeader(int runnNo, string customerName, int driverId, string signatureFileUrl, string imageFileUrl)
        {
            DateTime unloadDateTime = DateTime.Now;

            string query = @"
                INSERT INTO ""MobileDespatchConfirmationHeader""
                (""RunnNo"", ""CustomerName"", ""DriverID"", ""SignatureURL"", ""ImageConfirmationURL"", ""UnloadDateTime"") 
                VALUES (@RunnNo, @CustomerName, @DriverID, @SignatureURL, @ImageConfirmationURL, @UnloadDateTime)";

            DynamicParameters parameters = new DynamicParameters();
            parameters.Add("@RunnNo", runnNo);
            parameters.Add("@CustomerName", customerName);
            parameters.Add("@DriverID", driverId);
            parameters.Add("@SignatureURL", signatureFileUrl);
            parameters.Add("@ImageConfirmationURL", imageFileUrl);
            parameters.Add("@UnloadDateTime", unloadDateTime);

            _dapper.Execute(query, parameters);
        }

        public void InsertIntoDetailLines(int runnNo, int despatchId, string customerName)
        {
            string sql = @"
                SELECT r.""SerialBarcodeValue"" AS ""BarCodeValue""
                FROM ""spilRunnSheetDownloadedBarCodes"" r
                JOIN ""spilRunnSheetHeader"" sh ON r.""RunnNo"" = sh.""RunnNo""
                JOIN ""spilInvNum"" inv ON r.""OrderIndex"" = inv.""OrderIndex""
                WHERE sh.""RunnNo"" = @RunnNo AND inv.""cAccountName"" = @CustomerName AND r.""BarcodeScannedStatus"" = 3;
            ";

            var data = _dapper.Query<dynamic>(sql, new { RunnNo = runnNo, CustomerName = customerName });

            foreach (var item in data)
            {
                string barCodeValue = item.BarCodeValue;

                int orderIndex = GetOrderIndex(barCodeValue);
                int iInvDetailID = GetInvDetailID(barCodeValue);

                string insertQuery = @"
                    INSERT INTO ""MobileDespatchConfirmationDetailLines""
                    (""DespatchID"", ""RunnNo"", ""OrderIndex"", ""iInvDetailID"", ""BarCodeValue"")
                    VALUES (@DespatchID, @RunnNo, @OrderIndex, @iInvDetailID, @BarCodeValue)";

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@DespatchID", despatchId);
                parameters.Add("@RunnNo", runnNo);
                parameters.Add("@OrderIndex", orderIndex);
                parameters.Add("@iInvDetailID", iInvDetailID);
                parameters.Add("@BarCodeValue", barCodeValue);

                _dapper.Execute(insertQuery, parameters);
            }
        }

        private int GetOrderIndex(string barCodeValue)
        {
            string query = @"SELECT ""OrderIndex"" FROM ""spilPROD_SERIALS"" WHERE ""BarCodeV"" = @BarCodeValue";
            return _dapper.QueryFirstOrDefault<int>(query, new { BarCodeValue = barCodeValue });
        }

        private int GetInvDetailID(string barCodeValue)
        {
            string query = @"SELECT ""iInvDetailID"" FROM ""spilPROD_SERIALS"" WHERE ""BarCodeV"" = @BarCodeValue";
            return _dapper.QueryFirstOrDefault<int>(query, new { BarCodeValue = barCodeValue });
        }
    }
}
