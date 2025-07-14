namespace Scanner.Core.Domain.Entities.Loading
{
    public class ScanDetailsRequest
    {
        public int RunnNo { get; set; }
        public int LoadNo { get; set; }


        public string DriverName { get; set; }
        public string[] BarCodeValue { get; set; }
    }
}
