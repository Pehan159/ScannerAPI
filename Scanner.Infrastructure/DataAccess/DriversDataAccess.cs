using Scanner.Core.Domain.Dto;
using Scanner.Infrastructure.Data;

namespace Mobile_Scanner.DataAccess
{
    public class DriversDataAccess
    {
        private readonly DataContextDapper _dapper;

        public DriversDataAccess(DataContextDapper dapper)
        {
            _dapper = dapper;
        }

        // Version without passing connectionString
        public IEnumerable<DriverDto> GetAllDrivers()
        {
            string sql = @"SELECT DISTINCT ""Name"" AS ""DriverName"" 
                           FROM ""spilDriverMaster"" 
                           WHERE ""Name"" IS NOT NULL;";
            return _dapper.LoadData<DriverDto>(sql);
        }

        // Optional: version that accepts a connection string explicitly
        public IEnumerable<DriverDto> GetAllDrivers(string connectionString)
        {
            string sql = @"SELECT DISTINCT ""Name"" AS ""DriverName"" 
                           FROM ""spilDriverMaster"" 
                           WHERE ""Name"" IS NOT NULL;";
            return _dapper.LoadData<DriverDto>(sql, connectionString);
        }
    }
}
