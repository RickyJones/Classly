using Classly.Models;
using Classly.Models.Courses;
using MySqlConnector;

namespace Classly.Services
{
    public interface ICourseNotesService
    {
        // CREATE
        Task CreateCourseNoteAsync(CourseNote note);

        // READ (by Id)
        Task<CourseNote?> GetCourseNoteAsync(Guid id);

        // READ (all for a student)
        Task<List<CourseNote>> GetNotesForStudentAsync(Guid studentId);

        // UPDATE
        Task<bool> UpdateCourseNoteAsync(CourseNote note);

        // DELETE
        Task<bool> DeleteCourseNoteAsync(Guid id);
    }

    public class CourseNotesService: ICourseNotesService
    {
        private readonly string _connectionString;

        public CourseNotesService()
        {
            _connectionString = TestKeys.currentCon;
        }

        // CREATE
        public async Task CreateCourseNoteAsync(CourseNote note)
        {
            note.Id = Guid.NewGuid();
            note.CreatedAt = DateTime.UtcNow;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"INSERT INTO CourseNotes 
                       (Id, TutorId, StudentId, Difficulty, Notes, Homework, CreatedAt)
                       VALUES (@Id, @TutorId, @StudentId, @Difficulty, @Notes, @Homework, @CreatedAt)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", note.Id.ToString());
            cmd.Parameters.AddWithValue("@TutorId", note.TutorId.ToString());
            cmd.Parameters.AddWithValue("@StudentId", note.StudentId.ToString());
            cmd.Parameters.AddWithValue("@Difficulty", note.Difficulty);
            cmd.Parameters.AddWithValue("@Notes", note.Notes);
            cmd.Parameters.AddWithValue("@Homework", note.Homework);
            cmd.Parameters.AddWithValue("@CreatedAt", note.CreatedAt);

            await cmd.ExecuteNonQueryAsync();
        }

        // READ (by Id)
        public async Task<CourseNote?> GetCourseNoteAsync(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM CourseNotes WHERE Id = @Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CourseNote
                {
                    Id = reader.GetGuid("Id"),
                    TutorId = reader.GetGuid("TutorId"),
                    StudentId = reader.GetGuid("StudentId"),
                    Difficulty = reader.GetString("Difficulty"),
                    Notes = reader.GetString("Notes"),
                    Homework = reader.GetString("Homework"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                };
            }
            return null;
        }

        // UPDATE
        public async Task<bool> UpdateCourseNoteAsync(CourseNote note)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"UPDATE CourseNotes 
                       SET TutorId=@TutorId, StudentId=@StudentId, Difficulty=@Difficulty, 
                           Notes=@Notes, Homework=@Homework 
                       WHERE Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", note.Id.ToString());
            cmd.Parameters.AddWithValue("@TutorId", note.TutorId.ToString());
            cmd.Parameters.AddWithValue("@StudentId", note.StudentId.ToString());
            cmd.Parameters.AddWithValue("@Difficulty", note.Difficulty);
            cmd.Parameters.AddWithValue("@Notes", note.Notes);
            cmd.Parameters.AddWithValue("@Homework", note.Homework);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // DELETE
        public async Task<bool> DeleteCourseNoteAsync(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "DELETE FROM CourseNotes WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id.ToString());

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        public async Task<List<CourseNote>> GetNotesForStudentAsync(Guid studentId)
        {
            var notes = new List<CourseNote>();

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM CourseNotes WHERE studentId = @studentId";
            using var cmd = new MySqlCommand(sql, conn);

            // Pass as string if stored as CHAR(36)
            cmd.Parameters.AddWithValue("@studentId", studentId.ToString());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                notes.Add(new CourseNote
                {
                    Id = reader.GetGuid("Id"),
                    TutorId = reader.GetGuid("TutorId"),
                    StudentId = reader.GetGuid("StudentId"),
                    Difficulty = reader.GetString("Difficulty"),
                    Notes = reader.GetString("Notes"),
                    Homework = reader.GetString("Homework"),
                    CreatedAt = reader.GetDateTime("CreatedAt")
                });
            }

            return notes;
        }


    }
}
