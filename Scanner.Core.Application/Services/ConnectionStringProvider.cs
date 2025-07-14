using Microsoft.AspNetCore.Http;
using Scanner.Core.Application.Interfaces;
using Scanner.Infrastructure.Data;

namespace Scanner.Core.Application.Services
{
    public class ConnectionStringProvider : IConnectionStringProvider
    {
        private readonly TenantContext _context;
        private string _tenantId;

        public ConnectionStringProvider(IHttpContextAccessor httpContextAccessor, TenantContext context)
        {
            var tenantId = httpContextAccessor.HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).
                ToList().FirstOrDefault(x => x.Type == "TenantGuid")?.Value;
            _tenantId = tenantId;
            _context = context;
            //TO DO --> If tenant id is null or empty relog
        }

        public string GetConnectionString()
        {
            var tenant = _context.Tenants.FirstOrDefault(x => x.TenentGuid == _tenantId);
            if (tenant == null)
            {
                return string.Empty;
            }
            return tenant.ConnectionString;
        }
    }
}
