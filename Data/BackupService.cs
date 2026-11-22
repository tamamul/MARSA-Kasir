using Microsoft.Data.Sqlite;
namespace MarsaKasir.Data;

public static class BackupService
{
    public static string Folder
    {
        get
        {
            var f = SettingsRepository.Get("backup_folder", "");
            return f != "" ? f : Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "MarsaKasir-Backup");
        }
    }

    public static DateTime? LastBackup() =>
        DateTime.TryParse(SettingsRepository.Get("last_backup", ""), out var d) ? d : null;

    // Salinan utuh dan konsisten, aman walau database sedang dipakai
    public static string Run(string prefix = "kasir")
    {
        var folder = Folder;
        Directory.CreateDirectory(folder);
        var file = Path.Combine(folder, $"{prefix}-{DateTime.Now:yyyyMMdd-HHmmss}.db");
        using (var c = Db.Open())
        using (var cmd = c.CreateCommand())
        {
            cmd.CommandText = $"VACUUM INTO '{file.Replace("'", "''")}'";
            cmd.ExecuteNonQuery();
        }
        SettingsRepository.Set("last_backup", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        Prune(folder);
        return file;
    }

    // Dipanggil saat aplikasi dibuka dan ditutup; diam-diam, paling sering tiap 8 jam
    public static void Auto()
    {
        try
        {
            var last = LastBackup();
            if (last != null && (DateTime.Now - last.Value).TotalHours < 8) return;
            Run();
        }
        catch (Exception ex) { ActivityLog.Write("Backup gagal", ex.Message, "sistem"); }
    }

    static void Prune(string folder, int keep = 60)
    {
        foreach (var old in Directory.GetFiles(folder, "kasir-*.db").OrderByDescending(f => f).Skip(keep))
            try { File.Delete(old); } catch { }
    }

    public static void Restore(string file)
    {
        // pastikan file ini memang database MARSA KASIR
        using (var c = new SqliteConnection($"Data Source={file};Mode=ReadOnly;Pooling=False"))
        {
            c.Open();
            using var cmd = c.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM products";
            cmd.ExecuteScalar();
        }
        Run("sebelum-pulihkan");              // cadangan data saat ini, jaga-jaga
        SqliteConnection.ClearAllPools();
        File.Copy(file, Db.DbPath, overwrite: true);
        foreach (var ext in new[] { "-wal", "-shm" })
            if (File.Exists(Db.DbPath + ext)) File.Delete(Db.DbPath + ext);
    }
}
