using Classly.Models.Booking;
using Classly.Models.Courses;
using MySqlConnector;

namespace Classly.Services.Data
{
    public interface IBookingService
    {
        Task<List<Booking>> GetAllAsync();
        Task<Booking?> GetByIdAsync(Guid id);
        Task<List<Booking>> GetByUserIdAsync(Guid userId);
        Task<Booking> CreateAsync(Guid courseId, Guid userId);
        Task<bool> DeleteAsync(Guid bookingId);
    }

    public class BookingService : IBookingService
    {
        private readonly string _connectionString;
        private readonly IUserService _userService;
        private readonly ICourseService _courseService;

        public BookingService(string connectionString, IUserService userService, ICourseService courseService)
        {
            _connectionString = connectionString;
            _userService = userService;
            _courseService = courseService;
        }

        public async Task<List<Booking>> GetAllAsync()
        {
            var bookings = new List<Booking>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM bookings", conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new Booking
                {
                    Id = reader.GetGuid("id"),
                    Course = await _courseService.GetCourseByIdAsync(reader.GetGuid("courseid")),
                    BookedUser = await _userService.GetUser(reader.GetGuid("userid")),
                    BookedOnDate = reader.GetDateTime("bookedondate")
                });
            }

            return bookings;
        }

        public async Task<Booking?> GetByIdAsync(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM bookings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Booking
                {
                    Id = reader.GetGuid("id"),
                    Course = await _courseService.GetCourseByIdAsync(reader.GetGuid("courseid")),
                    BookedUser = await _userService.GetUser(reader.GetGuid("userid")),
                    BookedOnDate = reader.GetDateTime("bookedondate")
                };
            }

            return null;
        }

        public async Task<List<Booking>> GetByUserIdAsync(Guid userId)
        {
            var bookings = new List<Booking>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("SELECT * FROM bookings WHERE userid = @userId", conn);
            cmd.Parameters.AddWithValue("@userId", userId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                bookings.Add(new Booking
                {
                    Id = reader.GetGuid("id"),
                    Course = await _courseService.GetCourseByIdAsync(reader.GetGuid("courseid")),
                    BookedUser = await _userService.GetUser(reader.GetGuid("userid")),
                    BookedOnDate = reader.GetDateTime("bookedondate")
                });
            }

            return bookings;
        }

        public async Task<Booking> CreateAsync(Guid courseId, Guid userId)
        {
            var booking = new Booking
            {
                Id = Guid.NewGuid(),
                Course = new Course { Id = courseId},
                BookedUser = new Models.User { Id = userId},
                BookedOnDate = DateTime.UtcNow
            };

            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand(@"INSERT INTO bookings (id, courseid, userid, bookedondate)
                                     VALUES (@id, @courseId, @userId, @bookedOnDate)", conn);
            cmd.Parameters.AddWithValue("@id", booking.Id);
            cmd.Parameters.AddWithValue("@courseId", courseId);
            cmd.Parameters.AddWithValue("@userId", userId);
            cmd.Parameters.AddWithValue("@bookedOnDate", booking.BookedOnDate);

            cmd.ExecuteNonQuery();
            return booking;
        }

        public async Task<bool> DeleteAsync(Guid bookingId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            var cmd = new MySqlCommand("DELETE FROM bookings WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", bookingId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }

}
