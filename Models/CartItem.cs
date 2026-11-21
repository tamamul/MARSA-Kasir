namespace MarsaKasir.Models;

public class CartItem
{
    public long ProductId { get; set; }
    public string Barcode { get; set; } = "";
    public string Name { get; set; } = "";
    public long SellPrice { get; set; }
    public long BuyPrice { get; set; }   // disimpan untuk laporan keuntungan
    public int Qty { get; set; }
    public long Subtotal => SellPrice * Qty;
}