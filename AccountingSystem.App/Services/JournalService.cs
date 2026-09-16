using System.Globalization;
using AccountingSystem.App.Data;
using AccountingSystem.App.Models;

namespace AccountingSystem.App.Services;

public class JournalService
{
    private const string DateFormat = "yyyy-MM-dd";

    public List<JournalEntry> GetAll()
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            SELECT je.Id, je.EntryDate, je.AccountId, je.VoucherNo, je.Description, je.Debit, je.Credit,
                   a.Name, a.Category, a.Type
            FROM JournalEntries je
            JOIN Accounts a ON a.AccountId = je.AccountId
            ORDER BY je.EntryDate, je.VoucherNo, je.Id;
            """;

        var results = new List<JournalEntry>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new JournalEntry
            {
                Id = reader.GetInt32(0),
                EntryDate = DateTime.ParseExact(reader.GetString(1), DateFormat, CultureInfo.InvariantCulture),
                AccountId = reader.GetString(2),
                VoucherNo = reader.GetInt32(3),
                Description = reader.GetString(4),
                Debit = reader.GetDecimal(5),
                Credit = reader.GetDecimal(6),
                AccountName = reader.GetString(7),
                Category = reader.GetString(8),
                Type = reader.GetString(9)
            });
        }
        return results;
    }

    public List<JournalEntry> GetFilteredEntries(DateTime? from, DateTime? to, string? accountId)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();

        var where = new List<string>();
        if (from.HasValue) where.Add("je.EntryDate >= $from");
        if (to.HasValue) where.Add("je.EntryDate <= $to");
        if (!string.IsNullOrEmpty(accountId)) where.Add("je.AccountId = $accountId");

        var whereClause = where.Count > 0 ? "WHERE " + string.Join(" AND ", where) : string.Empty;

        cmd.CommandText = $"""
            SELECT je.Id, je.EntryDate, je.AccountId, je.VoucherNo, je.Description, je.Debit, je.Credit,
                   a.Name, a.Category, a.Type
            FROM JournalEntries je
            JOIN Accounts a ON a.AccountId = je.AccountId
            {whereClause}
            ORDER BY je.EntryDate, je.VoucherNo, je.Id;
            """;

        if (from.HasValue)
            cmd.Parameters.AddWithValue("$from", from.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
        if (to.HasValue)
            cmd.Parameters.AddWithValue("$to", to.Value.ToString(DateFormat, CultureInfo.InvariantCulture));
        if (!string.IsNullOrEmpty(accountId))
            cmd.Parameters.AddWithValue("$accountId", accountId);

        var results = new List<JournalEntry>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            results.Add(new JournalEntry
            {
                Id = reader.GetInt32(0),
                EntryDate = DateTime.ParseExact(reader.GetString(1), DateFormat, CultureInfo.InvariantCulture),
                AccountId = reader.GetString(2),
                VoucherNo = reader.GetInt32(3),
                Description = reader.GetString(4),
                Debit = reader.GetDecimal(5),
                Credit = reader.GetDecimal(6),
                AccountName = reader.GetString(7),
                Category = reader.GetString(8),
                Type = reader.GetString(9)
            });
        }
        return results;
    }

    public int GetNextVoucherNo()
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(MAX(VoucherNo), 200) + 1 FROM JournalEntries;";
        var result = cmd.ExecuteScalar();
        return result is long l ? (int)l : Convert.ToInt32(result);
    }

    public void Add(JournalEntry entry)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = """
            INSERT INTO JournalEntries (EntryDate, AccountId, VoucherNo, Description, Debit, Credit)
            VALUES ($date, $accountId, $voucherNo, $description, $debit, $credit);
            """;
        cmd.Parameters.AddWithValue("$date", entry.EntryDate.ToString(DateFormat, CultureInfo.InvariantCulture));
        cmd.Parameters.AddWithValue("$accountId", entry.AccountId);
        cmd.Parameters.AddWithValue("$voucherNo", entry.VoucherNo);
        cmd.Parameters.AddWithValue("$description", entry.Description);
        cmd.Parameters.AddWithValue("$debit", entry.Debit);
        cmd.Parameters.AddWithValue("$credit", entry.Credit);
        cmd.ExecuteNonQuery();
    }

    public void Delete(int id)
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "DELETE FROM JournalEntries WHERE Id = $id;";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }

    /// <summary>Sum of debits minus sum of credits across the whole journal. Should always be 0 if balanced.</summary>
    public decimal GetOutOfBalanceAmount()
    {
        using var connection = DbConnectionFactory.CreateConnection();
        using var cmd = connection.CreateCommand();
        cmd.CommandText = "SELECT COALESCE(SUM(Debit),0) - COALESCE(SUM(Credit),0) FROM JournalEntries;";
        var result = cmd.ExecuteScalar();
        return result is DBNull or null ? 0m : Convert.ToDecimal(result);
    }
}
