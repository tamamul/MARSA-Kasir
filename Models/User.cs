namespace MarsaKasir.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = "";
    public string Role { get; set; } = "kasir";   // "admin" / "kasir"
    public bool Active { get; set; } = true;
    public string Status => Active ? "Aktif" : "Nonaktif";
}