using System.Data;
using Microsoft.Data.Sqlite;
using System.IO;

namespace TaskManagement.DataAccess
{
    public class DapperContext
    {
        private readonly string _connectionString;
        public DapperContext()
        {
            var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "TaskManagement.db");
            _connectionString = $"Data Source={dbPath}";
            EnsureDatabaseCreated();
        }
        public IDbConnection CreateConnection() => new SqliteConnection(_connectionString);

        private void EnsureDatabaseCreated()
        {
            using var connection = CreateConnection();
                    connection.Open();
            using var command = connection.CreateCommand();
            // Issue Table
                    command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Issues (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT NOT NULL,
                Status TEXT NOT NULL,
                Priority TEXT,
                CreatedByUserId INTEGER NOT NULL,
                AssignedToUserId INTEGER,
                TaskId INTEGER,
                CreatedAt TEXT NOT NULL,
                ClosedAt TEXT,
                SolutionNote TEXT
            );
            CREATE TABLE IF NOT EXISTS AuditLogs (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Action TEXT NOT NULL,
                EntityType TEXT NOT NULL,
                EntityId INTEGER NOT NULL,
                Timestamp TEXT NOT NULL,
                Description TEXT NOT NULL
                        );
            CREATE TABLE IF NOT EXISTS IssueComments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IssueId INTEGER NOT NULL,
                UserId INTEGER NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL
                        );
            CREATE TABLE IF NOT EXISTS IssueAttachments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                IssueId INTEGER NOT NULL,
                FileName TEXT NOT NULL,
                FilePath TEXT NOT NULL,
                UploadedAt TEXT NOT NULL,
                UploadedByUserId INTEGER NOT NULL
                        );
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL,
                PasswordHash TEXT NOT NULL,
                Role TEXT NOT NULL,
                Email TEXT,
                Bio TEXT,
                LastActiveAt TEXT,
                IsApproved INTEGER NOT NULL DEFAULT 0
            );
            CREATE TABLE IF NOT EXISTS PasswordResetTokens (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                UserId INTEGER NOT NULL,
                Token TEXT NOT NULL,
                Expiration TEXT NOT NULL
            );
                    ";
                    command.ExecuteNonQuery();
        }
    }
}
