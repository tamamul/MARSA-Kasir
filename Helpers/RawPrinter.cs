using System.Drawing.Printing;
using System.Runtime.InteropServices;
using MarsaKasir.Data;
namespace MarsaKasir.Helpers;

public static class RawPrinter
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    class DOCINFO
    {
        [MarshalAs(UnmanagedType.LPWStr)] public string pDocName = "";
        [MarshalAs(UnmanagedType.LPWStr)] public string? pOutputFile;
        [MarshalAs(UnmanagedType.LPWStr)] public string pDataType = "RAW";
    }

    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool OpenPrinter(string name, out IntPtr h, IntPtr defaults);
    [DllImport("winspool.drv", SetLastError = true)] static extern bool ClosePrinter(IntPtr h);
    [DllImport("winspool.drv", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern bool StartDocPrinter(IntPtr h, int level, [In] DOCINFO di);
    [DllImport("winspool.drv", SetLastError = true)] static extern bool EndDocPrinter(IntPtr h);
    [DllImport("winspool.drv", SetLastError = true)] static extern bool StartPagePrinter(IntPtr h);
    [DllImport("winspool.drv", SetLastError = true)] static extern bool EndPagePrinter(IntPtr h);
    [DllImport("winspool.drv", SetLastError = true)]
    static extern bool WritePrinter(IntPtr h, byte[] data, int count, out int written);

    // Kirim byte mentah langsung ke printer (tanpa driver menggambar)
    public static void Send(string printerName, byte[] data)
    {
        if (!OpenPrinter(printerName, out var h, IntPtr.Zero))
            throw new InvalidOperationException("Printer tidak bisa dibuka: " + printerName);
        try
        {
            if (!StartDocPrinter(h, 1, new DOCINFO { pDocName = "MARSA KASIR" }))
                throw new InvalidOperationException("Printer menolak perintah.");
            try
            {
                StartPagePrinter(h);
                WritePrinter(h, data, data.Length, out _);
                EndPagePrinter(h);
            }
            finally { EndDocPrinter(h); }
        }
        finally { ClosePrinter(h); }
    }
}

public static class CashDrawer
{
    // Perintah ESC/POS "buka laci": ESC p <pin> <lama on> <lama off>
    public static void TryOpen()
    {
        try
        {
            var name = SettingsRepository.Get("printer", "");
            if (name == "") name = new PrinterSettings().PrinterName;
            byte pin = SettingsRepository.Get("drawer_pin", "0") == "1" ? (byte)1 : (byte)0;
            RawPrinter.Send(name, new byte[] { 0x1B, 0x70, pin, 0x19, 0xFA });
        }
        catch (Exception ex) { MessageBox.Show("Gagal membuka laci: " + ex.Message); }
    }
}
