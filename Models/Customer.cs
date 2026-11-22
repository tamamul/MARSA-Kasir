namespace MarsaKasir.Models;

public class Customer
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public string Note { get; set; } = "";
    public long CreditLimit { get; set; }   // 0 = tanpa batas
    public long Balance { get; set; }       // sisa kasbon (dihitung, tidak disimpan)
    public override string ToString() => Note == "" ? Name : $"{Name} ({Note})";
}
