using Classly.Models;
using Classly.Models.AIGen;
using Classly.Models.Config;
using Classly.Models.Courses;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Classly.Services
{
    public interface ICourseNotesService
    {
        AIPromptsModel GetAIPrompts();
        bool UpdateAIPrompts(AIPromptsModel model);

        // CREATE
        Task<CourseNote> CreateCourseNoteAsync(CourseNote note);

        // READ (by Id)
        Task<CourseNote?> GetCourseNoteAsync(Guid id);

        // READ (all for a student)
        Task<List<CourseNote>> GetNotesForStudentAsync(Guid studentId);

        // UPDATE
        Task<bool> UpdateCourseNoteAsync(CourseNote note);

        // DELETE
        Task<bool> DeleteCourseNoteAsync(Guid id);

        Task<List<CourseNote>> GetNotesWithoutHomeworkAsync(Guid studentId);
    }


    public class CourseNotesService : ICourseNotesService
    {
        private readonly string _connectionString;

        public CourseNotesService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<CourseNote>> GetNotesWithoutHomeworkAsync(Guid studentId)
        {
            var notes = new List<CourseNote>();

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"
        SELECT cn.* 
        FROM coursenotes cn
        WHERE cn.StudentId = @StudentId
        AND NOT EXISTS (
            SELECT 1 
            FROM homeworksubmission hw 
            WHERE hw.linkedCourseNoteId = cn.Id
        )";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId.ToString());
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
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    NextLessonPlan = reader.GetString("NextLessonPlan"),
                    Topic = reader.IsDBNull(reader.GetOrdinal("Topic")) ? null : reader.GetString("Topic")
                });
            }

            return notes;
        }


        // CREATE
        public async Task<CourseNote> CreateCourseNoteAsync(CourseNote note)
        {
            note.Id = Guid.NewGuid();
            note.CreatedAt = DateTime.UtcNow;

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"INSERT INTO courseNotes 
                       (Id, TutorId, StudentId, Difficulty, Notes, Homework, NextLessonPlan, CreatedAt, Topic)
                       VALUES (@Id, @TutorId, @StudentId, @Difficulty, @Notes, @Homework, @NextLessonPlan, @CreatedAt, @Topic)";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", note.Id.ToString());
            cmd.Parameters.AddWithValue("@TutorId", note.TutorId.ToString());
            cmd.Parameters.AddWithValue("@StudentId", note.StudentId.ToString());
            cmd.Parameters.AddWithValue("@Difficulty", note.Difficulty);
            cmd.Parameters.AddWithValue("@Notes", note.Notes);
            cmd.Parameters.AddWithValue("@Homework", note.Homework);
            cmd.Parameters.AddWithValue("@NextLessonPlan", note.NextLessonPlan);
            cmd.Parameters.AddWithValue("@CreatedAt", note.CreatedAt);
            cmd.Parameters.AddWithValue("@Topic", note.Topic ?? (object)DBNull.Value);

            await cmd.ExecuteNonQueryAsync();
            return note;
        }

        // READ (by Id)
        public async Task<CourseNote?> GetCourseNoteAsync(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM courseNotes WHERE Id = @Id";
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
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    NextLessonPlan = reader.GetString("NextLessonPlan"),
                    Topic = reader.IsDBNull(reader.GetOrdinal("Topic")) ? null : reader.GetString("Topic")
                };
            }
            return null;
        }

        // UPDATE
        public async Task<bool> UpdateCourseNoteAsync(CourseNote note)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = @"UPDATE courseNotes 
                       SET TutorId=@TutorId, StudentId=@StudentId, Difficulty=@Difficulty, 
                           Notes=@Notes, Homework=@Homework, NextLessonPlan=@NextLessonPlan, Topic=@Topic
                       WHERE Id=@Id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", note.Id.ToString());
            cmd.Parameters.AddWithValue("@TutorId", note.TutorId.ToString());
            cmd.Parameters.AddWithValue("@StudentId", note.StudentId.ToString());
            cmd.Parameters.AddWithValue("@Difficulty", note.Difficulty);
            cmd.Parameters.AddWithValue("@Notes", note.Notes);
            cmd.Parameters.AddWithValue("@Homework", note.Homework);
            cmd.Parameters.AddWithValue("@NextLessonPlan", note.NextLessonPlan);
            cmd.Parameters.AddWithValue("@Topic", note.Topic ?? (object)DBNull.Value);

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // DELETE
        public async Task<bool> DeleteCourseNoteAsync(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "DELETE FROM courseNotes WHERE Id=@Id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@Id", id.ToString());

            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // READ (all for a student)
        public async Task<List<CourseNote>> GetNotesForStudentAsync(Guid studentId)
        {
            var notes = new List<CourseNote>();

            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();

            string sql = "SELECT * FROM courseNotes WHERE StudentId = @StudentId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@StudentId", studentId.ToString());

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
                    CreatedAt = reader.GetDateTime("CreatedAt"),
                    NextLessonPlan = reader.GetString("NextLessonPlan"),
                    Topic = reader.IsDBNull(reader.GetOrdinal("Topic")) ? null : reader.GetString("Topic")
                });
            }

            return notes;
        }

        public AIPromptsModel GetAIPrompts()
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        SELECT AITablePrompt, AIHomewrokPrompt, AILessonPlanPrompt
        FROM AIPrompts
    ";

            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new AIPromptsModel
                {
                    AITablePrompt = reader["AITablePrompt"].ToString(),
                    AIHomewrokPrompt = reader["AIHomewrokPrompt"].ToString(),
                    AILessonPlanPrompt = reader["AILessonPlanPrompt"].ToString()
                };
            }

            return null; // not found
        }

        public bool UpdateAIPrompts(AIPromptsModel model)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string sql = @"
        UPDATE AIPrompts
        SET 
            AITablePrompt = @AITablePrompt,
            AIHomewrokPrompt = @AIHomewrokPrompt,
            AILessonPlanPrompt = @AILessonPlanPrompt
        WHERE Id = @Id;
    ";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@AITablePrompt", model.AITablePrompt);
            cmd.Parameters.AddWithValue("@AIHomewrokPrompt", model.AIHomewrokPrompt);
            cmd.Parameters.AddWithValue("@AILessonPlanPrompt", model.AILessonPlanPrompt);

            return cmd.ExecuteNonQuery() > 0;
        }

    }

}
