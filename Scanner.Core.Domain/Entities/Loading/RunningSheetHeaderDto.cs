namespace Scanner.Core.Domain.Entities.Loading
{
    public class RunningSheetHeaderDto
    {
        public int LoadNo { get; set; }

        public int RunnNo { get; set; }

        public string VehRegNo { get; set; }

        public string AreaDescription { get; set; }

        public int TotalCustomerCount { get; set; }

        public int TotalItemsToBeLoaded { get; set; }

        public double TotalWeightToBeLoaded { get; set; }
        public double VehichleMaxGlassWeight { get; set; }




    }
}
