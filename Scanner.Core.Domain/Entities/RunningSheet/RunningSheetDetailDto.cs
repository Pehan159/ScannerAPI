namespace Scanner.Core.Domain.Entities.RunningSheet
{
    public class RunningSheetDetailDto
    {
        public int RunnNo { get; set; }

        public int OrderIndex { get; set; }

        public string CustomerOrderNum { get; set; }
        public DateTime DueDate { get; set; }
        public string Address { get; set; }
        public string Customer { get; set; }
        public string ContactName { get; set; }
        public string ContactTelephone { get; set; }
        public int TotalGlassPanels { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public string DueDateOnly => DueDate.ToString("yyyy-MM-dd");

    }
}
