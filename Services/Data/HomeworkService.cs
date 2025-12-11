using Classly.Models;
using MySqlConnector;
using System;
using System.Collections.Generic;

namespace Classly.Services.Data
{
    public interface IHomeworkSubmissionService
    {
        // CREATE
        void AddSubmission(HomeworkSubmission hw);

        // READ ALL
        List<HomeworkSubmission> GetSubmissions();

        // READ ONE
        HomeworkSubmission GetSubmissionById(Guid id);
        HomeworkSubmission GetSubmissionForNote(Guid noteId);

        // UPDATE
        void UpdateSubmission(HomeworkSubmission hw);

        // DELETE
        void DeleteSubmission(string id);
    }

    public class HomeworkSubmissionService : IHomeworkSubmissionService
    {
        private readonly string _connectionString;

        public HomeworkSubmissionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // CREATE
        public void AddSubmission(HomeworkSubmission hw)
        {
            hw.Id = Guid.NewGuid();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            string sql = @"INSERT INTO homeworksubmission 
                       (id, content, createdAt, linkedCourseNoteId, markAsComplete) 
                       VALUES (@id, @content, @createdAt, @linkedCourseNoteId, @markAsComplete)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", hw.Id);
            cmd.Parameters.AddWithValue("@content", hw.Content);
            cmd.Parameters.AddWithValue("@createdAt", hw.CreatedAt);
            cmd.Parameters.AddWithValue("@linkedCourseNoteId", hw.LinkedCourseNoteId);
            cmd.Parameters.AddWithValue("@markAsComplete", hw.MarkAsComplete);
            cmd.ExecuteNonQuery();
        }

        // READ ALL
        public List<HomeworkSubmission> GetSubmissions()
        {
            var list = new List<HomeworkSubmission>();
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            string sql = "SELECT id, content, createdAt, linkedCourseNoteId, markAsComplete FROM homeworksubmission";
            using var cmd = new MySqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new HomeworkSubmission
                {
                    Id = reader.GetGuid("id"),
                    Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? (DateTime?)null : reader.GetDateTime("createdAt"),
                    LinkedCourseNoteId = reader.IsDBNull(reader.GetOrdinal("linkedCourseNoteId")) ? null : reader.GetString("linkedCourseNoteId"),
                    MarkAsComplete = reader.GetBoolean("markAsComplete")
                });
            }
            return list;
        }

        // READ ONE
        public HomeworkSubmission? GetSubmissionById(Guid id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();

            string sql = @"SELECT id, content, createdAt, linkedCourseNoteId, markAsComplete 
                   FROM homeworksubmission 
                   WHERE id = @id";

            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.Add("@id", MySqlDbType.VarChar).Value = id.ToString();

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new HomeworkSubmission
                {
                    Id = reader.GetGuid("id"),
                    Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? (DateTime?)null : reader.GetDateTime("createdAt"),
                    LinkedCourseNoteId = (reader.IsDBNull(reader.GetOrdinal("linkedCourseNoteId")) ? (Guid?)null : reader.GetGuid("linkedCourseNoteId")).ToString(),
                    MarkAsComplete = reader.IsDBNull(reader.GetOrdinal("markAsComplete")) ? false : reader.GetBoolean("markAsComplete")
                };
            }

            return null;
        }




        public HomeworkSubmission GetSubmissionForNote(Guid noteId)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            string sql = "SELECT id, content, createdAt, linkedCourseNoteId, markAsComplete FROM homeworksubmission WHERE linkedCourseNoteId=@noteId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@noteId", noteId);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new HomeworkSubmission
                {
                    Id = reader.GetGuid("id"),
                    Content = reader.IsDBNull(reader.GetOrdinal("content")) ? null : reader.GetString("content"),
                    CreatedAt = reader.IsDBNull(reader.GetOrdinal("createdAt")) ? (DateTime?)null : reader.GetDateTime("createdAt"),
                    LinkedCourseNoteId = reader.IsDBNull(reader.GetOrdinal("linkedCourseNoteId")) ? null : reader.GetString("linkedCourseNoteId"),
                    MarkAsComplete = reader.GetBoolean("markAsComplete")
                };
            }
            return null;
        }

        // UPDATE
        public void UpdateSubmission(HomeworkSubmission hw)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            string sql = @"UPDATE homeworksubmission 
                       SET content=@content, createdAt=@createdAt, linkedCourseNoteId=@linkedCourseNoteId 
                       WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@content", hw.Content);
            cmd.Parameters.AddWithValue("@createdAt", hw.CreatedAt);
            cmd.Parameters.AddWithValue("@linkedCourseNoteId", hw.LinkedCourseNoteId);
            cmd.Parameters.AddWithValue("@id", hw.Id);
            cmd.Parameters.AddWithValue("@markAsComplete", hw.MarkAsComplete);
            cmd.ExecuteNonQuery();
        }

        // DELETE
        public void DeleteSubmission(string id)
        {
            using var conn = new MySqlConnection(_connectionString);
            conn.Open();
            string sql = "DELETE FROM homeworksubmission WHERE id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
