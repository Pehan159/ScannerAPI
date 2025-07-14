namespace Scanner.Core.Entities
{
    public class TenantDevice
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string SecretKey { get; set; }
        public int TenantId { get; set; }
        public Tenant Tenant { get; set; }
    }
}