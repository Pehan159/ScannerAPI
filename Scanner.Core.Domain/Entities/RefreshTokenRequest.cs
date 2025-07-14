namespace Scanner.Core.Domain.Entities
{
    public class RefreshTokenRequest
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public string ApplicationId { get; set; }
    }
}
