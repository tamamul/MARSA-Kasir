namespace MarsaKasir.Models;

public class Product
{
    public long Id { get; set; }
    public string Barcode { get; set; } = "";
    public string Name { get; set; } = "";
    public long BuyPrice { get; set; }
    public long SellPrice { get; set; }
    public int Stock { get; set; }
    public int MinStock { get; set; }
    public string Unit { get; set; } = "pcs";
}