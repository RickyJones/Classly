using Classly.Models;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Xml.Linq;

namespace Classly.Services.Data
{
    public interface IUserService
    {
        public User? GetUser(string email);
        public Task<User> CreateAsync(User user, CancellationToken cancellationToken);
        public bool ValidatePassword(string inputPassword, string storedHash);
    }
    public class UserService: IUserService
    {
        public User? GetUser(string email)
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
                return new User
                {
                    Id = Guid.Parse(reader["id"].ToString() ?? "0"),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                    Password = reader["password"]?.ToString()
                };
            }

            Console.WriteLine("User not found.");
            return null;
        }

        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password); 

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "INSERT INTO users (name, email, password) VALUES (@name, @email, @password)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", user.Name); // Use actual username
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@password", hashedPassword);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected > 0)
            {
                return user;
            }
            else
            {
                return null;
            }
        }

        public async Task<bool> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "DELETE FROM users WHERE email = @Email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", user.Email);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }


        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public async Task<User?> FindByEmailAsync(string email, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "SELECT id, name, email, password FROM users WHERE LOWER(email) = LOWER(@NormalizedEmail)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@NormalizedEmail", email);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    Id = Guid.Parse(reader["id"].ToString()),
                    Name = reader["name"].ToString(),
                    Email = reader["email"].ToString(),
                    Password = reader["password"].ToString()
                };
            }

            return null;
        }


        public async Task<bool> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "UPDATE users SET name = @Name, email = @Email, password = @Password WHERE id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Name", user.Name);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.Password);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            return rowsAffected > 0;
        }

        public bool ValidatePassword(string inputPassword, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(inputPassword, storedHash);
        }
    }
}
