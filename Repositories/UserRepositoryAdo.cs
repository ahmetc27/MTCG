using Npgsql;
using System;
using System.Collections.Generic;
using MTCG.Models;

namespace MTCG.Repositories
{
    public class UserRepositoryAdo
    {
        private readonly string _connectionString;

        public UserRepositoryAdo(string connectionString)
        {
            _connectionString = connectionString;
        }

        // Methode: Alle User abrufen
        public List<User> GetAllUsers()
        {
            var users = new List<User>();

            using var connection = new NpgsqlConnection(_connectionString);
            connection.Open();

            var sql = "SELECT username, elo, wins, losses FROM users";
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                users.Add(new User
                {
                    Username = reader.GetString(0),
                    Elo = reader.GetInt32(1),
                    Wins = reader.GetInt32(2),
                    Losses = reader.GetInt32(3)
                });
            }

            return users;
        }
    }
}