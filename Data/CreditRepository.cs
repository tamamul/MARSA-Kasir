using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Data;

public record LedgerRow(string Time, string Info, long Kasbon, long Bayar);

public static class CreditRepository
{
    // Sisa kasbon = total kasbon belanja - total pembayaran
    public static List<Customer> Customers(string search = "")
    {
        var list = new List<Customer>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"SELECT c.id, c.name, c.note, c.credit_limit,
              COALESCE((SELECT SUM(credit_amount) FROM sales WHERE customer_id=c.id),0)
            - COALESCE((SELECT SUM(amount) FROM debt_payments WHERE customer_id=c.id),0)
            FROM customers c WHERE c.active=1 AND c.name LIKE $t ORDER BY c.name";
        cmd.Parameters.AddWithValue("$t", $"%{search}%");
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new Customer
            {
                Id = r.GetInt64(0), Name = r.GetString(1), Note = r.GetString(2),
                CreditLimit = r.GetInt64(3), Balance = r.GetInt64(4)
            });
        return list;
    }

    // Id = 0 berarti pelanggan baru; mengembalikan id-nya
    public static long SaveCustomer(Customer cu)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = cu.Id == 0
            ? "INSERT INTO customers(name,note,credit_limit) VALUES($n,$o,$l); SELECT last_insert_rowid();"
            : "UPDATE customers SET name=$n,note=$o,credit_limit=$l WHERE id=$id; SELECT $id;";
        cmd.Parameters.AddWithValue("$id", cu.Id);
        cmd.Parameters.AddWithValue("$n", cu.Name);
        cmd.Parameters.AddWithValue("$o", cu.Note);
        cmd.Parameters.AddWithValue("$l", cu.CreditLimit);
        return (long)cmd.ExecuteScalar()!;
    }

    public static void AddPayment(long customerId, long amount, string note)
    {
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"INSERT INTO debt_payments(customer_id,created_at,amount,note,username)
            VALUES($c,$t,$a,$n,$u)";
        cmd.Parameters.AddWithValue("$c", customerId);
        cmd.Parameters.AddWithValue("$t", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        cmd.Parameters.AddWithValue("$a", amount);
        cmd.Parameters.AddWithValue("$n", note);
        cmd.Parameters.AddWithValue("$u", Session.Current?.Username ?? "");
        cmd.ExecuteNonQuery();
    }

    // Gabungan kasbon belanja dan pembayaran, terbaru di atas
    public static List<LedgerRow> History(long customerId)
    {
        var list = new List<LedgerRow>();
        using var c = Db.Open();
        using var cmd = c.CreateCommand();
        cmd.CommandText = @"
            SELECT created_at, 'Kasbon belanja #' || id, credit_amount, 0 FROM sales
              WHERE customer_id=$c AND credit_amount>0
            UNION ALL
            SELECT created_at, CASE WHEN note<>'' THEN 'Bayar - ' || note ELSE 'Bayar' END, 0, amount
              FROM debt_payments WHERE customer_id=$c
            ORDER BY 1 DESC";
        cmd.Parameters.AddWithValue("$c", customerId);
        using var r = cmd.ExecuteReader();
        while (r.Read())
            list.Add(new LedgerRow(r.GetString(0), r.GetString(1), r.GetInt64(2), r.GetInt64(3)));
        return list;
    }
}
