using System.Drawing.Printing;
using MarsaKasir.Data;
using MarsaKasir.Models;
namespace MarsaKasir.Helpers;

public static class ReceiptPrinter
{
    // Dipakai kasir & riwayat: cetak berdasarkan nomor transaksi, error ditampilkan
    public static void TryPrint(long saleId)
    {
        try
        {
            var s = SaleRepository.Get(saleId);
            if (s != null) Print(s);
        }
        catch (Exception ex) { MessageBox.Show("Gagal cetak: " + ex.Message); }
    }

    public static void Print(Sale sale)
    {
        int widthMm = SettingsRepository.Get("paper", "58") == "80" ? 80 : 58;
        var printer = SettingsRepository.Get("printer", "");

        var doc = new PrintDocument { PrintController = new StandardPrintController() };
        if (printer != "") doc.PrinterSettings.PrinterName = printer;
        if (!doc.PrinterSettings.IsValid)
            throw new InvalidOperationException("Printer tidak ditemukan. Cek menu Pengaturan (F12).");
        doc.DefaultPageSettings.PaperSize =
            new PaperSize("Struk", (int)Math.Round(widthMm / 25.4 * 100), 3000);
        doc.DefaultPageSettings.Margins = new Margins(0, 0, 0, 0);

        doc.PrintPage += (_, e) =>
        {
            var g = e.Graphics!;
            using var font = new Font("Consolas", 8f);
            float printW = (widthMm == 80 ? 72f : 48f) / 25.4f * 100f;
            float charW = g.MeasureString("0000000000", font, 2000, StringFormat.GenericTypographic).Width / 10f;
            int cols = Math.Max(20, (int)(printW / charW));

            var lines = Build(sale, cols);
            float y = 5, lineH = font.GetHeight(g);
            foreach (var l in lines)
            {
                g.DrawString(l, font, Brushes.Black, 5, y, StringFormat.GenericTypographic);
                y += lineH;
            }
            e.HasMorePages = false;
        };
        doc.Print();
    }

    static List<string> Build(Sale s, int cols)
    {
        var lines = new List<string>();
        void Center(string t) { foreach (var w in Wrap(t, cols)) lines.Add(new string(' ', (cols - w.Length) / 2) + w); }
        void LR(string l, string r)
        {
            if (l.Length + r.Length >= cols) l = l[..Math.Max(0, cols - r.Length - 1)];
            lines.Add(l + new string(' ', cols - l.Length - r.Length) + r);
        }
        void Line() => lines.Add(new string('-', cols));

        Center(SettingsRepository.Get("store_name", "TOKO"));
        foreach (var a in SettingsRepository.Get("store_address").Split('\n')) Center(a);
        Line();
        lines.Add(s.Id == 0 ? "TES CETAK" : $"No : #{s.Id}");
        lines.Add("Tgl: " + s.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
        Line();
        foreach (var i in s.Items)
        {
            foreach (var w in Wrap(i.Name, cols)) lines.Add(w);
            LR($"  {i.Qty} x {Fmt.Num(i.SellPrice)}", Fmt.Num(i.Subtotal));
        }
        Line();
        LR("TOTAL", Fmt.Num(s.Total));
        LR("Bayar", Fmt.Num(s.Paid));
        LR("Kembali", Fmt.Num(s.Change));
        Line();
        Center(SettingsRepository.Get("footer", "Terima kasih"));
        lines.Add(""); lines.Add(""); lines.Add("");
        return lines;
    }

    static IEnumerable<string> Wrap(string s, int cols)
    {
        s = s.Trim();
        for (int i = 0; i < s.Length; i += cols)
            yield return s.Substring(i, Math.Min(cols, s.Length - i));
    }
}