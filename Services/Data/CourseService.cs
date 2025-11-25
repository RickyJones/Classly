using Classly.Models;
using Classly.Models.Courses;
using Classly.Models.Tutors;
using Microsoft.AspNetCore.Cors.Infrastructure;
using MySqlConnector;

namespace Classly.Services.Data
{
    public interface ICourseService
    {
        Task<List<Course>> GetAllCoursesAsync();
        Task<Course?> GetCourseByIdAsync(Guid id);
        Task AddCourseAsync(Course course);
        Task UpdateCourseAsync(Course course);
        Task DeleteCourseAsync(Guid id);
    }

    public class CourseService : ICourseService
    {
        private readonly string _connectionString;

        public CourseService()
        {
            _connectionString = TestKeys.currentCon;
        }

        public async Task<List<Course>> GetAllCoursesAsync()
        {
            var courses = new List<Course>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = "SELECT * FROM courses";
            using var command = new MySqlCommand(query, connection);
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                courses.Add(new Course
                {
                    Id = Guid.Parse(reader["id"].ToString() ?? "0"),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Description = reader["description"]?.ToString() ?? string.Empty,
                    Image = reader["imageurl"] != DBNull.Value ? new Uri(reader["imageurl"].ToString()!) : null,
                    Tutor = new Tutor { Id = Guid.Parse(reader["tutorid"].ToString() ?? "0") },
                    DateCommencing = DateOnly.FromDateTime(DateTime.Parse(reader["datecommencing"].ToString() ?? DateTime.MinValue.ToString())),
                    StartTime = TimeOnly.Parse(reader["starttime"].ToString() ?? TimeOnly.MinValue.ToString()),
                    EndTime = TimeOnly.Parse(reader["endtime"].ToString() ?? TimeOnly.MinValue.ToString()),
                    Price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : 0
                });
            }

            return courses;
        }

        public async Task<Course?> GetCourseByIdAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = "SELECT * FROM courses WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id.ToString());

            using var reader = await command.ExecuteReaderAsync();

            if (await reader.ReadAsync())
            {
                return new Course
                {
                    Id = Guid.Parse(reader["id"].ToString() ?? "0"),
                    Name = reader["name"]?.ToString() ?? string.Empty,
                    Description = reader["description"]?.ToString() ?? string.Empty,
                    Image = reader["imageurl"] != DBNull.Value ? new Uri(reader["imageurl"].ToString()!) : null,
                    Tutor = new Tutor { Id = Guid.Parse(reader["tutorid"].ToString() ?? "0") },
                    DateCommencing = DateOnly.Parse(reader["datecommencing"].ToString() ?? DateOnly.MinValue.ToString()),
                    StartTime = TimeOnly.Parse(reader["starttime"].ToString() ?? TimeOnly.MinValue.ToString()),
                    EndTime = TimeOnly.Parse(reader["endtime"].ToString() ?? TimeOnly.MinValue.ToString()),
                    Price = reader["price"] != DBNull.Value ? Convert.ToDecimal(reader["price"]) : 0
                };
            }

            return null;
        }

        public async Task AddCourseAsync(Course course)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"INSERT INTO courses 
            (id, name, description, imageurl, tutorid, datecommencing, starttime, endtime, price) 
            VALUES (@id, @name, @description, @imageurl, @tutorid, @datecommencing, @starttime, @endtime, @price)";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", course.Id.ToString());
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description);
            command.Parameters.AddWithValue("@imageurl", course.Image?.ToString());
            command.Parameters.AddWithValue("@tutorid", course.Tutor.Id.ToString());
            command.Parameters.AddWithValue("@datecommencing", course.DateCommencing.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@starttime", course.StartTime.ToString());
            command.Parameters.AddWithValue("@endtime", course.EndTime.ToString());
            command.Parameters.AddWithValue("@price", course.Price);

            await command.ExecuteNonQueryAsync();
        }

        public async Task UpdateCourseAsync(Course course)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = @"UPDATE courses SET 
            name = @name,
            description = @description,
            imageurl = @imageurl,
            tutorid = @tutorid,
            datecommencing = @datecommencing,
            starttime = @starttime,
            endtime = @endtime,
            price = @price
            WHERE id = @id";

            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", course.Id.ToString());
            command.Parameters.AddWithValue("@name", course.Name);
            command.Parameters.AddWithValue("@description", course.Description);
            command.Parameters.AddWithValue("@imageurl", course.Image?.ToString());
            command.Parameters.AddWithValue("@tutorid", course.Tutor.Id.ToString());
            command.Parameters.AddWithValue("@datecommencing", course.DateCommencing.ToString("yyyy-MM-dd"));
            command.Parameters.AddWithValue("@starttime", course.StartTime.ToString());
            command.Parameters.AddWithValue("@endtime", course.EndTime.ToString());
            command.Parameters.AddWithValue("@price", course.Price);

            await command.ExecuteNonQueryAsync();
        }

        public async Task DeleteCourseAsync(Guid id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            string query = "DELETE FROM courses WHERE id = @id";
            using var command = new MySqlCommand(query, connection);
            command.Parameters.AddWithValue("@id", id.ToString());

            await command.ExecuteNonQueryAsync();
        }
    }


}
