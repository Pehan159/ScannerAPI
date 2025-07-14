using Scanner.Core.Application.Interfaces;
using Scanner.Core.Dto;
using Scanner.Core.Entities;
using Scanner.Infrastructure.Data;
using System.Security.Cryptography;
using System.Text;

namespace Scanner.Application.Services
{
    public class TenantService : ITenantService
    {
        private readonly TenantContext _tenantContext;

        public TenantService(TenantContext tenantContext)
        {
            _tenantContext = tenantContext;
        }

        public string GetConnectionString(string tenentGuid)
        {
            var tenant = _tenantContext.Tenants.SingleOrDefault(t => t.TenentGuid == tenentGuid);
            return tenant?.ConnectionString;
        }

        public string AuthenticateUser(string username, string password)
        {
            var tenant = _tenantContext.Tenants.FirstOrDefault(t => t.Username == username && t.Password == password);
            return tenant?.TenentGuid;
        }

        public TenatAuthResponseModel AuthenticateTenantUser(string username, string password, string deviceId)
        {
            var result = new TenatAuthResponseModel();
            var tenant = _tenantContext.Tenants.FirstOrDefault(t => t.Username == username && t.Password == password);
            result.TenantGuid = tenant?.TenentGuid;
            if (tenant != null)
            {
                var tenatDevice = _tenantContext.TenantDevices.FirstOrDefault(t => t.TenantId == tenant.Id && t.DeviceId == deviceId);
                if (tenatDevice != null)
                {
                    result.SecretKey = tenatDevice.SecretKey;
                }
                else
                {
                    var secretKey = GenerateSecretKey(deviceId);
                    var newDevice = new TenantDevice
                    {
                        DeviceId = deviceId,
                        TenantId = tenant.Id,
                        SecretKey = secretKey
                    };

                    _tenantContext.TenantDevices.Add(newDevice);
                    _tenantContext.SaveChanges();
                    result.SecretKey = newDevice.SecretKey;
                }
            }
            return result;
        }

        public TenantDevice ValidateTenantDevice(string tenantGuid, string secretKey) 
        {
            var tenant = _tenantContext.Tenants.SingleOrDefault(t => t.TenentGuid == tenantGuid);
            if (tenant == null)
            {
                return null;
            }

            return _tenantContext.TenantDevices
                .FirstOrDefault(td => td.TenantId == tenant.Id && td.SecretKey == secretKey);
        }

        private static string GenerateID(string deviceId)
        {
            return string.Format("{0}_{1:N}", deviceId, System.Guid.NewGuid());
        }

        private string GenerateSecretKey(string uniqueString)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] uniqueStringBytes = Encoding.UTF8.GetBytes(uniqueString);
                byte[] hashBytes = sha256.ComputeHash(uniqueStringBytes);

                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }

                return hashStringBuilder.ToString();
            }
        }
    }
}
