namespace MarsaKasir.Data;

public static class SettingsRepository
{
    public static string Get(string key, string def = "")
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "SELECT value FROM settings WHERE key=$k";
        cmd.Parameters.AddWithValue("$k", key);
        return cmd.ExecuteScalar() as string ?? def;
    }

    public static void Set(string key, string value)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"INSERT INTO settings(key,value) VALUES($k,$v)
            ON CONFLICT(key) DO UPDATE SET value=excluded.value";
        cmd.Parameters.AddWithValue("$k", key);
        cmd.Parameters.AddWithValue("$v", value);
        cmd.ExecuteNonQuery();
    }
}