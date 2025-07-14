using Mobile_Scanner.DataAccess;
using Scanner.Core.Application.Interfaces;
using Scanner.Core.Domain.Dto;
using Scanner.Infrastructure.Data;

namespace Scanner.Application.Services
{
    public class DriversService : IDriversService
    {
        private readonly TenantContext _tenantContext;
        private readonly DriversDataAccess _dataAccess;

        public DriversService(TenantContext tenantContext, DriversDataAccess dataAccess)
        {
            _tenantContext = tenantContext;
            _dataAccess = dataAccess;
        }



        public IEnumerable<DriverDto> GetDriversList(string tenantGuid, string deviceId, string secretKey)
        {
            var tenantDevice = _tenantContext.TenantDevices.FirstOrDefault(x =>  x.DeviceId == deviceId && x.SecretKey==secretKey);
            if (tenantDevice == null)
            {
                throw new UnauthorizedAccessException("Error Unauthorized Access");
            }
            var tenant = _tenantContext.Tenants.FirstOrDefault(x => x.Id == tenantDevice.TenantId && x.TenentGuid == tenantGuid);
            if (tenant == null)
            {
                throw new UnauthorizedAccessException("Error Unauthorized Access");
            }

            var drivers = _dataAccess.GetAllDrivers(tenant.ConnectionString);
            return drivers;
        }
    }
}
