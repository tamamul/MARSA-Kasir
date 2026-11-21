using MarsaKasir.Helpers;
namespace MarsaKasir.Data;

public record LogRow(string Time, string User, string Action, string Detail);

public static class ActivityLog
{
    // Log tidak boleh membuat aplikasi error, jadi semua kegagalan diabaikan
    public static void Write(string action, string detail = "", string? user = null)
    {
        try
        {
            using var c = Db.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = @"INSERT INTO activity_log(created_at,username,action,detail)
                VALUES($t,$u,$a,$d)";
            cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("$u", user ?? Session.Current?.Username ?? "-");
            cmd.Parameters.AddWithValue("$a", action);
            cmd.Parameters.AddWithValue("$d", detail);
            cmd.ExecuteNonQuery();
        }
        catch { }
    }

    public static List<LogRow> Recent(int limit = 500)
    {
        var list = new List<LogRow>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT created_at,username,action,detail FROM activity_log
            ORDER BY id DESC LIMIT $l";
        cmd.Parameters.AddWithValue("$l", limit);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(new LogRow(r.GetString(0), r.GetString(1), r.GetString(2), r.GetString(3)));
        return list;
    }
}