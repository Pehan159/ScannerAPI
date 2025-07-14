using Microsoft.EntityFrameworkCore;

namespace Scanner.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }




        
    }
}
