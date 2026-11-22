namespace MarsaKasir.Models;

public class SaleLine
{
    public string Name { get; set; } = "";
    public int Qty { get; set; }
    public long SellPrice { get; set; }
    public long Subtotal => Qty * SellPrice;
}

public class Sale
{
    public long Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public long Total { get; set; }
    public long Paid { get; set; }
    public long Change { get; set; }
    public List<SaleLine> Items { get; set; } = new();
    public long Credit { get; set; }
    public string Customer { get; set; } = "";
}
