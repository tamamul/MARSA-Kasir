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

    public static string DbPath => Path.Combine(Folder, "kasir.db");

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
CREATE INDEX IF NOT EXISTS ix_products_name ON products(name);
CREATE TABLE IF NOT EXISTS sales (
  id            INTEGER PRIMARY KEY AUTOINCREMENT,
  created_at    TEXT NOT NULL,
  total         INTEGER NOT NULL,
  paid          INTEGER NOT NULL,
  change_amount INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS sale_items (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  sale_id    INTEGER NOT NULL REFERENCES sales(id),
  product_id INTEGER NOT NULL,
  name       TEXT NOT NULL,
  qty        INTEGER NOT NULL,
  sell_price INTEGER NOT NULL,
  buy_price  INTEGER NOT NULL
);
CREATE TABLE IF NOT EXISTS settings (
  key   TEXT PRIMARY KEY,
  value TEXT NOT NULL
);
CREATE TABLE IF NOT EXISTS users (
  id       INTEGER PRIMARY KEY AUTOINCREMENT,
  username TEXT NOT NULL UNIQUE COLLATE NOCASE,
  salt     TEXT NOT NULL,
  hash     TEXT NOT NULL,
  role     TEXT NOT NULL DEFAULT 'kasir',
  active   INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS activity_log (
  id         INTEGER PRIMARY KEY AUTOINCREMENT,
  created_at TEXT NOT NULL,
  username   TEXT NOT NULL,
  action     TEXT NOT NULL,
  detail     TEXT NOT NULL DEFAULT ''
);
CREATE TABLE IF NOT EXISTS customers (
  id           INTEGER PRIMARY KEY AUTOINCREMENT,
  name         TEXT NOT NULL,
  note         TEXT NOT NULL DEFAULT '',
  credit_limit INTEGER NOT NULL DEFAULT 0,
  active       INTEGER NOT NULL DEFAULT 1
);
CREATE TABLE IF NOT EXISTS debt_payments (
  id          INTEGER PRIMARY KEY AUTOINCREMENT,
  customer_id INTEGER NOT NULL REFERENCES customers(id),
  created_at  TEXT NOT NULL,
  amount      INTEGER NOT NULL,
  note        TEXT NOT NULL DEFAULT '',
  username    TEXT NOT NULL DEFAULT ''
);
";
        cmd.ExecuteNonQuery();
        AddColumn(c, "sales", "customer_id", "INTEGER");
        AddColumn(c, "sales", "credit_amount", "INTEGER NOT NULL DEFAULT 0");
    }
    static void AddColumn(SqliteConnection c, string table, string column, string type)
    {
        using var q = c.CreateCommand();
        q.CommandText = $"SELECT COUNT(*) FROM pragma_table_info('{table}') WHERE name='{column}'";
        if ((long)q.ExecuteScalar()! > 0) return;
        using var a = c.CreateCommand();
        a.CommandText = $"ALTER TABLE {table} ADD COLUMN {column} {type}";
        a.ExecuteNonQuery();
    }
}
