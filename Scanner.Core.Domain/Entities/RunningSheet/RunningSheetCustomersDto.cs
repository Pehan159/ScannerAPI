namespace Scanner.Core.Domain.Entities.RunningSheet
{
    public class RunningSheetCustomersDto
    {
        public string Customer { get; set; }
        public string Address { get; set; }
        public int TotalOrders { get; set; }
        public decimal TotalWeight { get; set; }
        public int TotalQuantity { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
