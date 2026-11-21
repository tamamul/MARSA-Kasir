using Microsoft.Data.Sqlite;
namespace MarsaKasir.Data;

public record Summary(long Trx, long Omzet, long Profit);
public record PeriodRow(string Period, long Trx, long Omzet, long Profit);
public record TopRow(string Name, long Qty, long Omzet, long Profit);
public record StockRow(string Name, int Stock, int MinStock, string Unit);

public static class ReportRepository
{
    const string From = @"FROM sales s JOIN sale_items i ON i.sale_id=s.id
        WHERE date(s.created_at) BETWEEN $f AND $t";

    static List<T> Run<T>(string sql, Func<SqliteDataReader, T> map,
        string? from = null, string? to = null, string? fmt = null)
    {
        var list = new List<T>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = sql;
        if (from != null) cmd.Parameters.AddWithValue("$f", from);
        if (to != null) cmd.Parameters.AddWithValue("$t", to);
        if (fmt != null) cmd.Parameters.AddWithValue("$fmt", fmt);
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(map(r));
        return list;
    }

    // from/to format "yyyy-MM-dd"
    public static Summary GetSummary(string from, string to) => Run(
        $@"SELECT COUNT(DISTINCT s.id), COALESCE(SUM(i.qty*i.sell_price),0),
                  COALESCE(SUM(i.qty*(i.sell_price-i.buy_price)),0) {From}",
        r => new Summary(r.GetInt64(0), r.GetInt64(1), r.GetInt64(2)), from, to)[0];

    public static List<PeriodRow> ByPeriod(string from, string to, bool monthly) => Run(
        $@"SELECT strftime($fmt, s.created_at) AS p, COUNT(DISTINCT s.id),
                  SUM(i.qty*i.sell_price), SUM(i.qty*(i.sell_price-i.buy_price))
           {From} GROUP BY p ORDER BY p DESC",
        r => new PeriodRow(r.GetString(0), r.GetInt64(1), r.GetInt64(2), r.GetInt64(3)),
        from, to, monthly ? "%Y-%m" : "%Y-%m-%d");

    public static List<TopRow> TopProducts(string from, string to) => Run(
        $@"SELECT MAX(i.name), SUM(i.qty), SUM(i.qty*i.sell_price),
                  SUM(i.qty*(i.sell_price-i.buy_price))
           {From} GROUP BY i.product_id ORDER BY SUM(i.qty) DESC LIMIT 100",
        r => new TopRow(r.GetString(0), r.GetInt64(1), r.GetInt64(2), r.GetInt64(3)),
        from, to);

    public static List<StockRow> LowStock() => Run(
        @"SELECT name, stock, min_stock, unit FROM products
          WHERE active=1 AND stock <= min_stock ORDER BY stock, name",
        r => new StockRow(r.GetString(0), r.GetInt32(1), r.GetInt32(2), r.GetString(3)));
}