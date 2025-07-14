namespace Scanner.Core.Entities
{
    public class Tenant
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string TenentGuid { get; set; }
        public string ConnectionString { get; set; }
    }
}
