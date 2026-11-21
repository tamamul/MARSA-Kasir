using System.Globalization;
namespace MarsaKasir.Helpers;

public static class Fmt
{
    static readonly CultureInfo Id = CultureInfo.GetCultureInfo("id-ID");
    public static string Rp(long v) => "Rp " + v.ToString("N0", Id);
}