namespace Scanner.Core.Domain.Dto.Unloading
{
    public class UnloadingCustomersDto
    {
        public string Customer { get; set; }

        public string Address { get; set; }

        public int TotalItemsLoaded { get; set; }


        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
