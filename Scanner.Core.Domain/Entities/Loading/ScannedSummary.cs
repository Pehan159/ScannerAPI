namespace Scanner.Core.Domain.Entities.Loading
{
    public class ScannedSummary
    {
        public int RunnNo { get; set; }

        public string ExtOrderNum { get; set; }

        public string ItemDescription { get; set; }
        public string BarCodeValue { get; set; }

        public int BarcodeScannedStatus { get; set; }

        public int fQuantity { get; set; }
        public int iHeight { get; set; }
        public int iWidth { get; set; }




    }
}
