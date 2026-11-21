using Microsoft.Data.Sqlite;
namespace MarsaKasir.Data;

public static class Db
{
    static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "MarsaKasir");

    public static SqliteConnection Open()
    {
        Directory.CreateDirectory(Folder);
        var c = new SqliteConnection($"Data Source={Path.Combine(Folder, "kasir.db")}");
        c.Open();
        using var p = c.CreateCommand();
        p.CommandText = "PRAGMA journal_mode=WAL; PRAGMA foreign_keys=ON;";
        p.ExecuteNonQuery();
        return c;
    }

    public static void Init()
    {
        using var c = Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS products (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  barcode    TEXT NOT NULL UNIQUE,
  name       TEXT NOT NULL,
  buy_price  INTEGER NOT NULL DEFAULT 0,
  sell_price INTEGER NOT NULL DEFAULT 0,
  stock      INTEGER NOT NULL DEFAULT 0,
  min_stock  INTEGER NOT NULL DEFAULT 0,
  unit       TEXT NOT NULL DEFAULT 'pcs',
  active     INTEGER NOT NULL DEFAULT 1
);
CREATE INDEX IF NOT EXISTS ix_products_name ON products(name);";
        cmd.ExecuteNonQuery();
    }
}