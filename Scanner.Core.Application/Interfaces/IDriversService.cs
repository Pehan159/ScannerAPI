using Scanner.Core.Domain.Dto;

namespace Scanner.Core.Application.Interfaces
{
    public interface IDriversService
    {
        IEnumerable<DriverDto> GetDriversList(string tenantGuid, string deviceId, string secretKey);
    }
}