using System.Drawing.Printing;
using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class SettingsForm : Form
{
    readonly TextBox txtName = new() { Dock = DockStyle.Fill };
    readonly TextBox txtAddr = new() { Dock = DockStyle.Fill, Multiline = true, Height = 60 };
    readonly TextBox txtFooter = new() { Dock = DockStyle.Fill };
    readonly ComboBox cmbPaper = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly ComboBox cmbPrinter = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    readonly CheckBox chkAuto = new() { Text = "Cetak struk otomatis setelah bayar", AutoSize = true };

    public SettingsForm()
    {
        Text = "Pengaturan";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(480, 380);

        cmbPaper.Items.AddRange(new object[] { "58 mm", "80 mm" });
        cmbPrinter.Items.Add("(Printer default Windows)");
        foreach (string p in PrinterSettings.InstalledPrinters) cmbPrinter.Items.Add(p);

        txtName.Text = SettingsRepository.Get("store_name", "TOKO");
        txtAddr.Text = SettingsRepository.Get("store_address");
        txtFooter.Text = SettingsRepository.Get("footer", "Terima kasih");
        cmbPaper.SelectedIndex = SettingsRepository.Get("paper", "58") == "80" ? 1 : 0;
        var saved = SettingsRepository.Get("printer");
        cmbPrinter.SelectedIndex = Math.Max(0, cmbPrinter.Items.IndexOf(saved));
        chkAuto.Checked = SettingsRepository.Get("auto_print", "0") == "1";

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(string label, Control c)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            t.Controls.Add(c);
        }
        Row("Nama toko", txtName);
        Row("Alamat", txtAddr);
        Row("Teks bawah", txtFooter);
        Row("Ukuran kertas", cmbPaper);
        Row("Printer", cmbPrinter);
        t.Controls.Add(chkAuto);
        t.SetColumnSpan(chkAuto, 2);

        var btnSave = new Button { Text = "Simpan", Width = 90 };
        var btnTest = new Button { Text = "Tes cetak", Width = 90 };
        var btnCancel = new Button { Text = "Batal", Width = 90, DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) => { Save(); DialogResult = DialogResult.OK; };
        btnTest.Click += (_, _) => { Save(); TestPrint(); };
        var bar = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 44, Padding = new Padding(8) };
        bar.Controls.AddRange(new Control[] { btnCancel, btnSave, btnTest });

        Controls.Add(t);
        Controls.Add(bar);
        CancelButton = btnCancel;
    }

    void Save()
    {
        SettingsRepository.Set("store_name", txtName.Text.Trim());
        SettingsRepository.Set("store_address", txtAddr.Text.Trim());
        SettingsRepository.Set("footer", txtFooter.Text.Trim());
        SettingsRepository.Set("paper", cmbPaper.SelectedIndex == 1 ? "80" : "58");
        SettingsRepository.Set("printer", cmbPrinter.SelectedIndex <= 0 ? "" : cmbPrinter.Text);
        SettingsRepository.Set("auto_print", chkAuto.Checked ? "1" : "0");
        ActivityLog.Write("Ubah pengaturan");
    }

    static void TestPrint()
    {
        var s = new Sale { Id = 0, CreatedAt = DateTime.Now, Total = 25000, Paid = 30000, Change = 5000 };
        s.Items.Add(new SaleLine { Name = "Contoh Produk Dengan Nama Panjang Sekali", Qty = 2, SellPrice = 7500 });
        s.Items.Add(new SaleLine { Name = "Produk B", Qty = 1, SellPrice = 10000 });
        try { ReceiptPrinter.Print(s); }
        catch (Exception ex) { MessageBox.Show("Gagal cetak: " + ex.Message); }
    }
}
