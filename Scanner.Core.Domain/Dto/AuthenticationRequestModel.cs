namespace Scanner.Core.Dto
{
    public class AuthenticationRequestModel 
    {
        public string TenentGuid { get; set; }
        public string SecretKey { get; set; }
        public string DeviceId { get; set; }
    }
}
