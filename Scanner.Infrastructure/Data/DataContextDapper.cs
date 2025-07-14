using Dapper;
using Npgsql;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Scanner.Core.Application.Interfaces;
using System.Data;



namespace Scanner.Infrastructure.Data
{
    public class DataContextDapper
    {
        private readonly IConfiguration _config;
        //private readonly IConnectionStringProvider _connectionStringProvider;
        private readonly string _connectionString;
        public DataContextDapper(IConfiguration config, IConnectionStringProvider connectionStringProvider)
        {
            _config = config;
            _connectionString = connectionStringProvider.GetConnectionString();
        }

        public IEnumerable<T> LoadData<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
            return dbConnection.Query<T>(sql, parameters);
        }

        public IEnumerable<T> LoadData<T>(string sql, string connectionString, object parameters = null)
        {
            using IDbConnection dbConnection = new NpgsqlConnection(connectionString);

            return dbConnection.Query<T>(sql, parameters);
        }


        public T LoadDataSingle<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QueryFirstOrDefault<T>(sql, parameters);
        }

        public bool ExecuteSql(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql) > 0;
        }

        public int ExecuteSqlWithRowCount(string sql)
        {
            IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Execute(sql);
        }
        public bool ExecuteSqlWithParameters(string sql, List<SqlParameter> parameters)
        {
            SqlCommand commandWithParams = new SqlCommand(sql);

            foreach (SqlParameter parameter in parameters)
            {
                commandWithParams.Parameters.Add(parameter);
            }

            SqlConnection dbConnection = new SqlConnection(_connectionString);
            dbConnection.Open();

            commandWithParams.Connection = dbConnection;

            int rowsAffected = commandWithParams.ExecuteNonQuery();

            dbConnection.Close();

            return rowsAffected > 0;
        }

        public T ExecuteScalar<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
            return dbConnection.ExecuteScalar<T>(sql, parameters);
        }

        public int Execute(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new NpgsqlConnection(_connectionString);
            return dbConnection.Execute(sql, parameters);
        }


        public T QueryFirstOrDefault<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QueryFirstOrDefault<T>(sql, parameters);
        }

        public IEnumerable<T> Query<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.Query<T>(sql, parameters);
        }

        public T QuerySingleOrDefault<T>(string sql, object parameters = null)
        {
            using IDbConnection dbConnection = new SqlConnection(_connectionString);
            return dbConnection.QuerySingleOrDefault<T>(sql, parameters);
        }

    }
}
