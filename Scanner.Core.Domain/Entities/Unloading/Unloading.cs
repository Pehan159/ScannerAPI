namespace Scanner.Core.Domain.Entities.Unloading
{
    public class Unloading
    {
        public int RunnNo { get; set; }
        public int LoadNo { get; set; }
        public string VehRegNo { get; set; }
        public string AreaDescription { get; set; }
        public int TotalCustomerCount { get; set; }
        public int TotalLoadedItems { get; set; }

        public double TotalWeight { get; set; }




    }

public class UnloadingRequest {
    public string CustomerName { get; set; }
    public int RunnNo       { get; set; }
    public List<string> Barcode { get; set; }
}
}
