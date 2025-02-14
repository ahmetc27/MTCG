using Npgsql;
using Dapper;
using System;
using System.Collections.Generic;

namespace MTCG.Repositories
{
    public class DatabaseRepository
    {
        private readonly string _connectionString;

        public DatabaseRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<dynamic> ExecuteQuery(string sql)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Query(sql);
        }

        public int ExecuteNonQuery(string sql)
        {
            using var connection = new NpgsqlConnection(_connectionString);
            return connection.Execute(sql);
        }
    }
}