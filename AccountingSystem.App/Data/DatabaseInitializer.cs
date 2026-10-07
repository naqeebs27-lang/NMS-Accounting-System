using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Sqlite;

namespace AccountingSystem.App.Data;

/// <summary>
/// Creates the SQLite schema on first run and seeds it with the data imported
/// from the original "Dashboard_Accounting_System" Excel workbook
/// (Chart of Accounts + General Ledger sheets).
/// </summary>
public static class DatabaseInitializer
{
    private const string SchemaSql = """
        CREATE TABLE IF NOT EXISTS Accounts (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            AccountId TEXT NOT NULL UNIQUE,
            Name TEXT NOT NULL,
            Category TEXT NOT NULL,
            Type TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS JournalEntries (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            EntryDate TEXT NOT NULL,
            AccountId TEXT NOT NULL,
            VoucherNo INTEGER NOT NULL,
            Description TEXT NOT NULL,
            Debit REAL NOT NULL DEFAULT 0,
            Credit REAL NOT NULL DEFAULT 0,
            FOREIGN KEY (AccountId) REFERENCES Accounts(AccountId)
        );

        CREATE INDEX IF NOT EXISTS IX_JournalEntries_AccountId ON JournalEntries(AccountId);
        CREATE INDEX IF NOT EXISTS IX_JournalEntries_EntryDate ON JournalEntries(EntryDate);
        """;

    public static void Initialize()
    {
        using var connection = DbConnectionFactory.CreateConnection();

        using (var schemaCmd = connection.CreateCommand())
        {
            schemaCmd.CommandText = SchemaSql;
            schemaCmd.ExecuteNonQuery();
        }

        var accountCount = ExecuteScalarLong(connection, "SELECT COUNT(*) FROM Accounts;");
        if (accountCount == 0)
        {
            RunEmbeddedScript(connection, "ChartOfAccounts.seed.sql");
        }

        using (var resetJournalCommand = connection.CreateCommand())
        {
            resetJournalCommand.CommandText = "DELETE FROM JournalEntries;";
            resetJournalCommand.ExecuteNonQuery();
        }

        RunEmbeddedScript(connection, "JournalEntries.seed.sql");
    }

    private static long ExecuteScalarLong(SqliteConnection connection, string sql)
    {
        using var cmd = connection.CreateCommand();
        cmd.CommandText = sql;
        var result = cmd.ExecuteScalar();
        return result is long l ? l : Convert.ToInt64(result);
    }

    private static void RunEmbeddedScript(SqliteConnection connection, string fileName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .FirstOrDefault(n => n.EndsWith(fileName, StringComparison.OrdinalIgnoreCase));

        if (resourceName is null)
        {
            throw new InvalidOperationException($"Embedded seed script '{fileName}' was not found.");
        }

        using var stream = assembly.GetManifestResourceStream(resourceName)!;
        using var reader = new StreamReader(stream);
        var script = reader.ReadToEnd();

        using var transaction = connection.BeginTransaction();
        using var cmd = connection.CreateCommand();
        cmd.Transaction = transaction;
        cmd.CommandText = script;
        cmd.ExecuteNonQuery();
        transaction.Commit();
    }
}
