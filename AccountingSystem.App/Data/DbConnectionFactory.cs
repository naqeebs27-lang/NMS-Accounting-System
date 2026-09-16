using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace AccountingSystem.App.Data;

/// <summary>
/// Creates connections to the local SQLite database file.
/// The database lives next to the executable, e.g. %LocalAppData%\AccountingSystem\accounting.db
/// </summary>
public static class DbConnectionFactory
{
    public static string DatabasePath { get; } = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "AccountingSystem",
        "accounting.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";

    public static SqliteConnection CreateConnection()
    {
        var directory = Path.GetDirectoryName(DatabasePath)!;
        Directory.CreateDirectory(directory);

        var connection = new SqliteConnection(ConnectionString);
        connection.Open();
        // Enforce foreign keys for every connection (SQLite has this off by default).
        using var pragma = connection.CreateCommand();
        pragma.CommandText = "PRAGMA foreign_keys = ON;";
        pragma.ExecuteNonQuery();
        return connection;
    }
}
