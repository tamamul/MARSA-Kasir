using MarsaKasir.Models;
namespace MarsaKasir.Data;

public static class SaleRepository
{
    public static long Save(IEnumerable<CartItem> items, long total, long paid)
    {
        using var c = Db.Open();
        using var tx = c.BeginTransaction();

        long saleId;
        using (var cmd = c.CreateCommand())
        {
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT INTO sales(created_at,total,paid,change_amount)
                VALUES($t,$tot,$p,$ch); SELECT last_insert_rowid();";
            cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.Parameters.AddWithValue("$tot", total);
            cmd.Parameters.AddWithValue("$p", paid);
            cmd.Parameters.AddWithValue("$ch", paid - total);
            saleId = (long)cmd.ExecuteScalar()!;
        }

        foreach (var i in items)
        {
            using var ins = c.CreateCommand();
            ins.Transaction = tx;
            ins.CommandText = @"INSERT INTO sale_items(sale_id,product_id,name,qty,sell_price,buy_price)
                VALUES($s,$pid,$n,$q,$sp,$bp)";
            ins.Parameters.AddWithValue("$s", saleId);
            ins.Parameters.AddWithValue("$pid", i.ProductId);
            ins.Parameters.AddWithValue("$n", i.Name);
            ins.Parameters.AddWithValue("$q", i.Qty);
            ins.Parameters.AddWithValue("$sp", i.SellPrice);
            ins.Parameters.AddWithValue("$bp", i.BuyPrice);
            ins.ExecuteNonQuery();

            using var upd = c.CreateCommand();
            upd.Transaction = tx;
            upd.CommandText = "UPDATE products SET stock = stock - $q WHERE id=$pid";
            upd.Parameters.AddWithValue("$q", i.Qty);
            upd.Parameters.AddWithValue("$pid", i.ProductId);
            upd.ExecuteNonQuery();
        }

        tx.Commit();
        return saleId;
    }
}