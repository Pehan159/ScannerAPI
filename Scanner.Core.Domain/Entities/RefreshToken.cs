namespace Scanner.Core.Domain.Entities
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserName { get; set; }
        public DateTime Expiration { get; set; }
    }
}
