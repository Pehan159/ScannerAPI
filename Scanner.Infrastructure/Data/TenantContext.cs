using Microsoft.EntityFrameworkCore;
using Scanner.Core.Domain.Entities;
using Scanner.Core.Entities;

namespace Scanner.Infrastructure.Data
{
    public class TenantContext : DbContext
    {
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<TenantDevice> TenantDevices { get; set; }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        public TenantContext(DbContextOptions<TenantContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);            
        }
    }
}
