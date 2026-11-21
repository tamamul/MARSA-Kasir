using MarsaKasir.Models;
namespace MarsaKasir.Helpers;

public static class Session
{
    public static User? Current { get; set; }
    public static bool IsAdmin => Current?.Role == "admin";
}