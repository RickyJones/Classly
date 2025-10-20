using Classly.Models;
using MySqlConnector;

namespace Classly.Services.Data
{
    public static class UserService
    {
        public static User? GetUser(string email)
        {
            using var connection = new MySqlConnection(TestKeys.localDBCon);
            connection.Open();

            string query = "SELECT * FROM users WHERE email = @email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);

            using var reader = command.ExecuteReader();

            if (reader.Read())
            {
                Console.WriteLine($"ID: {reader["id"]}, Name: {reader["name"]}, Email: {reader["email"]}");
                return new User { 
                    Id = int.Parse(reader["id"].ToString() ?? "0"),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                };
            }

            Console.WriteLine("User not found.");
            return null;
        }

        public static bool RegisterUser(string name, string email, string password)
        {
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            connection.Open();

            string query = "INSERT INTO users (name, email, password) VALUES (@name, @email, @password)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@password", hashedPassword);

            return command.ExecuteNonQuery() > 0;
        }

        public static bool VerifyLogin(string email, string password)
        {
            using var connection = new MySqlConnection(TestKeys.localDBCon);
            connection.Open();

            string query = "SELECT password FROM users WHERE email = @email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@email", email);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                string storedHash = reader.GetString("password");
                return BCrypt.Net.BCrypt.Verify(password, storedHash);
            }

            return false;
        }

    }
}
