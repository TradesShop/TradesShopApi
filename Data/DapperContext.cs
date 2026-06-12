using System.Data;
using Microsoft.Data.SqlClient;

namespace TradePlatform.Api.Data
{
    public class DapperContext
    {
        private readonly string _connectionString;

        public DapperContext(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("_devConnection")
                ?? throw new InvalidOperationException("Connection string '_devConnection' not found.");
        }

        /// <summary>
        /// Creates a new SQL connection (closed).
        /// </summary>
        public IDbConnection CreateConnection()
            => new Microsoft.Data.SqlClient.SqlConnection(_connectionString);

        /// <summary>
        /// Creates and opens a new SQL connection.
        /// This is the recommended method for all Dapper repositories.
        /// </summary>
        public IDbConnection CreateOpenConnection()
        {
            var conn = new Microsoft.Data.SqlClient.SqlConnection(_connectionString);
            conn.Open();
            return conn;
        }
    }
}
