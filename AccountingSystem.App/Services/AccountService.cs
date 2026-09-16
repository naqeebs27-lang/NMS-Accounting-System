using AccountingSystem.App.Data;
using AccountingSystem.App.Models;
using Microsoft.Data.Sqlite;

namespace AccountingSystem.App.Services;

public class AccountService
{
    public List<Account> GetAll()
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT Id, AccountId, Name, Category, Type FROM Accounts ORDER BY AccountId;";

        var results = new List<Account>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new Account
            {
                Id = reader.GetInt32(0),
                AccountId = reader.GetString(1),
                Name = reader.GetString(2),
                Category = reader.GetString(3),
                Type = reader.GetString(4)
            });
        }
        return results;
    }

    public void Add(Account account)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO Accounts (AccountId, Name, Category, Type)
            VALUES ($accountId, $name, $category, $type);
            """;
        cmd.Parameters.AddWithValue("$accountId", account.AccountId);
        cmd.Parameters.AddWithValue("$name", account.Name);
        cmd.Parameters.AddWithValue("$category", account.Category);
        cmd.Parameters.AddWithValue("$type", account.Type);
        cmd.ExecuteNonQuery();
    }

    public void Update(Account account)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            UPDATE Accounts
            SET AccountId = $accountId, Name = $name, Category = $category, Type = $type
            WHERE Id = $id;
            """;
        cmd.Parameters.AddWithValue("$accountId", account.AccountId);
        cmd.Parameters.AddWithValue("$name", account.Name);
        cmd.Parameters.AddWithValue("$category", account.Category);
        cmd.Parameters.AddWithValue("$type", account.Type);
        cmd.Parameters.AddWithValue("$id", account.Id);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM Accounts WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Returns true if the account is referenced by any journal entry.</summary>
    public bool IsInUse(string accountId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM JournalEntries WHERE AccountId = $accountId;";
        cmd.Parameters.AddWithValue("$accountId", accountId);
        var count = (long)cmd.ExecuteScalar()!;
        return count > 0;
    }
}
