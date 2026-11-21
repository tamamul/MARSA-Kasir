using Microsoft.Data.Sqlite;
using MarsaKasir.Models;
namespace MarsaKasir.Data;

public static class ProductRepository
{
    const string Cols = "id,barcode,name,buy_price,sell_price,stock,min_stock,unit";

    static Product Read(SqliteDataReader r) => new()
    {
        Id = r.GetInt64(0), Barcode = r.GetString(1), Name = r.GetString(2),
        BuyPrice = r.GetInt64(3), SellPrice = r.GetInt64(4),
        Stock = r.GetInt32(5), MinStock = r.GetInt32(6), Unit = r.GetString(7)
    };

    // Dipakai scanner: barcode persis sama
    public static Product? GetByBarcode(string barcode)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = $"SELECT {Cols} FROM products WHERE barcode=$b AND active=1";
        cmd.Parameters.AddWithValue("$b", barcode);
        using var r = cmd.ExecuteReader();
        return r.Read() ? Read(r) : null;
    }

    // Cari manual: nama atau barcode mengandung teks
    public static List<Product> Search(string text)
    {
        var list = new List<Product>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = $@"SELECT {Cols} FROM products
            WHERE active=1 AND (name LIKE $t OR barcode LIKE $t)
            ORDER BY name LIMIT 100";
        cmd.Parameters.AddWithValue("$t", $"%{text}%");
        using var r = cmd.ExecuteReader();
        while (r.Read()) list.Add(Read(r));
        return list;
    }

    // Id = 0 berarti produk baru, selain itu update
    public static void Save(Product p)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = p.Id == 0
            ? @"INSERT INTO products(barcode,name,buy_price,sell_price,stock,min_stock,unit)
                VALUES($b,$n,$bp,$sp,$s,$m,$u)"
            : @"UPDATE products SET barcode=$b,name=$n,buy_price=$bp,sell_price=$sp,
                stock=$s,min_stock=$m,unit=$u WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", p.Id);
        cmd.Parameters.AddWithValue("$b", p.Barcode);
        cmd.Parameters.AddWithValue("$n", p.Name);
        cmd.Parameters.AddWithValue("$bp", p.BuyPrice);
        cmd.Parameters.AddWithValue("$sp", p.SellPrice);
        cmd.Parameters.AddWithValue("$s", p.Stock);
        cmd.Parameters.AddWithValue("$m", p.MinStock);
        cmd.Parameters.AddWithValue("$u", p.Unit);
        cmd.ExecuteNonQuery();
    }

    // Hapus lunak, supaya riwayat penjualan nanti tidak rusak
    public static void Deactivate(long id)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = "UPDATE products SET active=0 WHERE id=$id";
        cmd.Parameters.AddWithValue("$id", id);
        cmd.ExecuteNonQuery();
    }
}