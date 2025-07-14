using Dapper;
using Microsoft.Data.SqlClient;
using Npgsql;
using Scanner.Core.Domain.Entities;

namespace Scanner.Infrastructure.DataAccess
{
    public class RefreshTokenRepository
    {
        public async Task<bool> AddRefreshTokenAsync(string connectionString, RefreshToken refreshToken)
        {
            string sql = @"INSERT INTO ""RefreshTokens"" (""Token"", ""UserName"", ""Expiration"") 
               VALUES (@Token, @UserName, @Expiration);";


            using (var connection = new NpgsqlConnection(connectionString))
            {
                var result = await connection.ExecuteAsync(sql, new
                {
                    refreshToken.Token,
                    refreshToken.UserName,
                    refreshToken.Expiration
                });

                return result > 0;
            }
        }
    }
}
