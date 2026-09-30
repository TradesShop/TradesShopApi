using System;
using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace TradePlatform.Api.Data;

public class DapperContext
{
	private readonly string _connectionString;

	public DapperContext(IConfiguration config)
	{
		_connectionString = config.GetConnectionString("_devConnection") ?? throw new InvalidOperationException("Connection string '_devConnection' not found.");
	}

	public IDbConnection CreateConnection()
	{
		return new SqlConnection(_connectionString);
	}

	public IDbConnection CreateOpenConnection()
	{
		SqlConnection conn = new SqlConnection(_connectionString);
		conn.Open();
		return conn;
	}
}
