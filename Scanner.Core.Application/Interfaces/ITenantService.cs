using Scanner.Core.Dto;

namespace Scanner.Core.Application.Interfaces
{
    public interface ITenantService
    {
        string GetConnectionString(string tenentGuid);
        string AuthenticateUser(string username, string password);
        TenatAuthResponseModel AuthenticateTenantUser(string username, string password, string deviceId);


    }
}
