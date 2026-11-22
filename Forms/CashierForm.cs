using System.ComponentModel;
using System.Media;
using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class CashierForm : BaseForm
{
    readonly BindingList<CartItem> cart = new();
    readonly TextBox txtBarcode = new()
        { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 20), PlaceholderText = "Scan barcode / ketik nama..." };
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, RowHeadersVisible = false,
        Font = new Font("Segoe UI", 13), RowTemplate = { Height = 34 }
    };
    readonly Label lblTotal = new()
        { Dock = DockStyle.Right, Width = 460, Font = new Font("Segoe UI", 28, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight };
    readonly Label lblStatus = new() { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Font = new Font("Segoe UI", 12) };

    public bool LoggedOut { get; private set; }
    static readonly Font StatusFont = new("Segoe UI", 12);
    static readonly Font DoneFont = new("Segoe UI", 18, FontStyle.Bold);
    public CashierForm()
    {
        Text = $"MARSA KASIR - {Session.Current?.Username} ({Session.Current?.Role})";
        WindowState = FormWindowState.Maximized;
        KeyPreview = true;
        lblTotal.ForeColor = Theme.Success;

        void Col(string title, string prop, int w, bool fill = false)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            if (prop != "Name" && prop != "Qty") c.DefaultCellStyle.Format = "N0";
            grid.Columns.Add(c);
        }
        Col("Nama", "Name", 300, fill: true);
        Col("Harga", "SellPrice", 140);
        Col("Qty", "Qty", 80);
        Col("Subtotal", "Subtotal", 160);
        grid.DataSource = cart;

        var bottom = new Panel { Dock = DockStyle.Fill, Height = 70 };
        bottom.Controls.Add(lblStatus);   // Fill ditambah dulu
        bottom.Controls.Add(lblTotal);

        var hint = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(4, 3, 4, 3) };
        void Key(string label, Keys key, bool adminOnly = false)
        {
            if (adminOnly && !Session.IsAdmin) return;   // kasir tidak melihat tombol admin
            var b = new Button { Text = label, AutoSize = true, TabStop = false };
            b.Click += (_, _) => { OnKey(null, new KeyEventArgs(key)); txtBarcode.Focus(); };
            hint.Controls.Add(b);
        }
        Key("F8 Bayar", Keys.F8);
        Key("F9 Kosongkan", Keys.F9);
        Key("F11 Riwayat", Keys.F11);
        Key("F10 Produk", Keys.F10, true);
        Key("F7 Laporan", Keys.F7, true);
        Key("F12 Pengaturan", Keys.F12, true);
        Key("F6 Pengguna", Keys.F6, true);
        Key("F5 Log", Keys.F5, true);
        Key("F4 Ganti user", Keys.F4);
        
        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 70));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46));
        root.Controls.Add(txtBarcode, 0, 0);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(bottom, 0, 2);
        root.Controls.Add(hint, 0, 3);
        Controls.Add(root);

        txtBarcode.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; Scan(); }
        };
        KeyDown += OnKey;
        Shown += (_, _) => { UpdateTotal(); txtBarcode.Focus(); };
    }

    void OnKey(object? s, KeyEventArgs e)
    {
        switch (e.KeyCode)
        {
            case Keys.F7 when Session.IsAdmin:
                using (var rp = new ReportForm()) rp.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
        	case Keys.F8: Pay(); e.Handled = true; break;
            case Keys.F9: ClearCart(); e.Handled = true; break;
            case Keys.F10 when Session.IsAdmin:
                using (var f = new ProductForm()) f.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
                            case Keys.F11:
                using (var h = new SalesHistoryForm()) h.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F12 when Session.IsAdmin:
            using (var st = new SettingsForm()) st.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F4: Logout(); e.Handled = true; break;
            case Keys.F5 when Session.IsAdmin:
                using (var lg = new ActivityLogForm()) lg.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F6 when Session.IsAdmin:
                using (var us = new UserForm()) us.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.Add: ChangeQty(+1); e.SuppressKeyPress = true; break;
            case Keys.Subtract: ChangeQty(-1); e.SuppressKeyPress = true; break;
            case Keys.Up: SelectRow((grid.CurrentRow?.Index ?? 0) - 1); e.Handled = true; break;
            case Keys.Down: SelectRow((grid.CurrentRow?.Index ?? 0) + 1); e.Handled = true; break;
            case Keys.Delete when txtBarcode.Text == "": RemoveRow(); e.Handled = true; break;
        }
    }

    void Scan()
    {
        var text = txtBarcode.Text.Trim();
        if (text == "") return;

        int qty = 1;
        var star = text.IndexOf('*');
        if (star > 0 && int.TryParse(text[..star], out var q) && q > 0)
        {
            qty = q;
            text = text[(star + 1)..].Trim();
        }

        var p = ProductRepository.GetByBarcode(text);
        if (p == null)
        {
            var found = ProductRepository.Search(text);
            if (found.Count == 1) p = found[0];
        }
        if (p == null)
        {
            Status($"Tidak ditemukan / terlalu banyak hasil: {text}", error: true);
            SystemSounds.Beep.Play();
            txtBarcode.SelectAll();
            return;
        }

        AddProduct(p, qty);
        Status($"{p.Name}  x{qty}");
        txtBarcode.Clear();
    }

    void AddProduct(Product p, int qty)
    {
        var idx = cart.ToList().FindIndex(x => x.ProductId == p.Id);
        if (idx >= 0) { cart[idx].Qty += qty; cart.ResetItem(idx); }
        else
        {
            cart.Add(new CartItem
            {
                ProductId = p.Id, Barcode = p.Barcode, Name = p.Name,
                SellPrice = p.SellPrice, BuyPrice = p.BuyPrice, Qty = qty
            });
            idx = cart.Count - 1;
        }
        UpdateTotal();
        SelectRow(idx);
    }

    void ChangeQty(int delta)
    {
        var idx = grid.CurrentRow?.Index ?? -1;
        if (idx < 0) return;
        cart[idx].Qty += delta;
        if (cart[idx].Qty <= 0) { RemoveRow(); return; }
        cart.ResetItem(idx);
        UpdateTotal();
    }

    void RemoveRow()
    {
        var idx = grid.CurrentRow?.Index ?? -1;
        if (idx < 0) return;
        ActivityLog.Write("Hapus baris", $"{cart[idx].Name} x{cart[idx].Qty}");
        cart.RemoveAt(idx);
        UpdateTotal();
        SelectRow(Math.Min(idx, cart.Count - 1));
    }

    void SelectRow(int i)
    {
        if (i < 0 || i >= grid.Rows.Count) return;
        grid.CurrentCell = grid.Rows[i].Cells[0];
    }

    void UpdateTotal() => lblTotal.Text = Fmt.Rp(cart.Sum(x => x.Subtotal));

    void ClearCart()
    {
        if (cart.Count == 0) return;
        if (MessageBox.Show("Kosongkan keranjang?", "Konfirmasi", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        ActivityLog.Write("Kosongkan keranjang", $"{cart.Count} baris, {Fmt.Rp(cart.Sum(x => x.Subtotal))}");
        cart.Clear();
        UpdateTotal();
        txtBarcode.Focus();
    }

    void Pay()
    {
        if (cart.Count == 0) return;
        long total = cart.Sum(x => x.Subtotal);
        using var f = new PaymentForm(total);
        if (f.ShowDialog(this) != DialogResult.OK) { txtBarcode.Focus(); return; }
        
        var id = SaleRepository.Save(cart, total, f.Paid);
        ActivityLog.Write("Transaksi", $"#{id} total {Fmt.Rp(total)}");
        if (SettingsRepository.Get("auto_print", "0") == "1") ReceiptPrinter.TryPrint(id); 
        cart.Clear();
        UpdateTotal();
        lblStatus.Font = DoneFont;
        lblStatus.ForeColor = Theme.Success;
        lblStatus.Text = $"Transaksi #{id} selesai  |  Kembalian: {Fmt.Rp(f.Paid - total)}";
        txtBarcode.Focus();
    }
    void Logout()
    {
        var msg = cart.Count > 0
            ? "Keranjang yang belum dibayar akan hilang. Ganti user?"
            : "Ganti user / keluar akun?";
        if (MessageBox.Show(msg, "Konfirmasi", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        ActivityLog.Write("Logout");
        LoggedOut = true;
        Close();
    }
    void Status(string text, bool error = false)
    {
        lblStatus.Font = StatusFont;
        lblStatus.ForeColor = error ? Theme.Danger : Theme.Ink;
        lblStatus.Text = text;
    }
}
