using Classly.Models;
using Microsoft.AspNetCore.Identity;
using MySqlConnector;
using System.Xml.Linq;

namespace Classly.Services.Data
{
    public interface IUserService
    {
        public User? GetUser(string email);
        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken);
    }
    public class UserService: IUserService, IUserStore<User>, IUserPasswordStore<User>
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
                    Id = int.Parse(reader["id"].ToString() ?? "0"),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Email = reader["email"]?.ToString() ?? string.Empty,
                };
            }

            Console.WriteLine("User not found.");
            return null;
        }

        //public static bool RegisterUser(string name, string email, string password)
        //{
        //    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

        //    using var connection = new MySqlConnection(TestKeys.localDBCon);
        //    connection.Open();

        //    string query = "INSERT INTO users (name, email, password) VALUES (@name, @email, @password)";
        //    using var command = new MySqlCommand(query, connection);
        //    command.Parameters.AddWithValue("@email", email);
        //    command.Parameters.AddWithValue("@name", name);
        //    command.Parameters.AddWithValue("@password", hashedPassword);

        //    return command.ExecuteNonQuery() > 0;
        //}

        //public static bool VerifyLogin(string email, string password)
        //{
        //    using var connection = new MySqlConnection(TestKeys.localDBCon);
        //    connection.Open();

        //    string query = "SELECT password FROM users WHERE email = @email";
        //    using var command = new MySqlCommand(query, connection);
        //    command.Parameters.AddWithValue("@email", email);

        //    using var reader = command.ExecuteReader();
        //    if (reader.Read())
        //    {
        //        string storedHash = reader.GetString("password");
        //        return BCrypt.Net.BCrypt.Verify(password, storedHash);
        //    }

        //    return false;
        //}

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash); // Use PasswordHash property

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "INSERT INTO users (name, email, password) VALUES (@name, @email, @password)";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@name", user.UserName); // Use actual username
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@password", hashedPassword);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected > 0)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Failed to insert user into database."
                });
            }
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "DELETE FROM users WHERE email = @Email";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Email", user.Email);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected > 0)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User with email '{user.Email}' could not be deleted or does not exist."
                });
            }
        }


        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        public async Task<User?> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "SELECT id, name, email, password FROM users WHERE id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", userId);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    Id = int.Parse(reader["id"].ToString()),
                    UserName = reader["name"].ToString(),
                    Email = reader["email"].ToString(),
                    PasswordHash = reader["password"].ToString()
                };
            }

            return null;
        }


        public async Task<User?> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "SELECT id, name, email, password FROM users WHERE LOWER(name) = @NormalizedUserName";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@NormalizedUserName", normalizedUserName);

            using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (await reader.ReadAsync(cancellationToken))
            {
                return new User
                {
                    Id = int.Parse(reader["id"].ToString()),
                    UserName = reader["name"].ToString(),
                    Email = reader["email"].ToString(),
                    PasswordHash = reader["password"].ToString()
                };
            }

            return null;
        }


        public Task<string?> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.UserName?.ToLowerInvariant());
        }


        public Task<string?> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.PasswordHash);
        }


        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.Id.ToString());
        }


        public Task<string?> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(user.UserName);
        }


        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.FromResult(!string.IsNullOrEmpty(user.PasswordHash));
        }


        public Task SetNormalizedUserNameAsync(User user, string? normalizedName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }


        public Task SetPasswordHashAsync(User user, string? passwordHash, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }


        public Task SetUserNameAsync(User user, string? userName, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            user.UserName = userName;
            return Task.CompletedTask;
        }


        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var connection = new MySqlConnection(TestKeys.localDBCon);
            await connection.OpenAsync(cancellationToken);

            string query = "UPDATE users SET name = @Name, email = @Email, password = @Password WHERE id = @Id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@Id", user.Id);
            command.Parameters.AddWithValue("@Name", user.UserName);
            command.Parameters.AddWithValue("@Email", user.Email);
            command.Parameters.AddWithValue("@Password", user.PasswordHash);

            int rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);

            if (rowsAffected > 0)
            {
                return IdentityResult.Success;
            }
            else
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = $"User with ID '{user.Id}' could not be updated."
                });
            }
        }

    }
}
