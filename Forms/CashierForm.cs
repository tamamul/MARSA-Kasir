using System.ComponentModel;
using System.Media;
using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class CashierForm : BaseForm
{
    // ---- palet warna & font ----
    static readonly Color Navy = Color.FromArgb(30, 41, 59);
    static readonly Color Ink = Color.FromArgb(17, 24, 39);
    static readonly Color Muted = Color.FromArgb(107, 114, 128);
    static readonly Color LineColor = Color.FromArgb(229, 231, 235);
    static readonly Color Green = Color.FromArgb(22, 163, 74);
    static readonly Color Red = Color.FromArgb(220, 38, 38);
    static readonly Color Soft = Color.FromArgb(243, 244, 246);
    static readonly Color Pick = Color.FromArgb(219, 234, 254);
    static readonly Font StatusFont = new("Segoe UI", 11);
    static readonly Font DoneFont = new("Segoe UI", 16, FontStyle.Bold);

    // ---- kontrol ----
    readonly BindingList<CartItem> cart = new();
    readonly TextBox txtBarcode = new()
    {
        Dock = DockStyle.Top, Font = new Font("Segoe UI", 18), BorderStyle = BorderStyle.FixedSingle,
        PlaceholderText = "Scan barcode / ketik nama produk..."
    };
    readonly Panel leftPane = new() { Dock = DockStyle.Fill, Padding = new Padding(16) };
    readonly Panel searchBox = new() { Dock = DockStyle.Top, Height = 80, Padding = new Padding(0, 0, 0, 12) };
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false, Tag = "keep"
    };
    readonly DataGridView sug = new()   // daftar saran pencarian (melayang di atas keranjang)
    {
        AutoGenerateColumns = false, ReadOnly = true, AllowUserToAddRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        RowHeadersVisible = false, Tag = "keep"
    };
    readonly Label lblEmpty = new()
    {
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, ForeColor = Muted,
        Font = new Font("Segoe UI", 14),
        Text = "Keranjang kosong\nScan barcode atau ketik nama produk untuk mulai"
    };
    readonly Label lblTotal = new()
    {
        Dock = DockStyle.Fill, Font = new Font("Segoe UI", 30, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft, ForeColor = Ink
    };
    readonly Label lblItems = new() { AutoSize = true, ForeColor = Muted, Font = new Font("Segoe UI", 11) };
    readonly Label lblStatus = new()
    {
        Dock = DockStyle.Fill, AutoSize = false, Font = StatusFont, ForeColor = Ink,
        Margin = new Padding(0, 10, 0, 0)
    };
    readonly System.Windows.Forms.Timer searchTimer = new() { Interval = 150 };

    public bool LoggedOut { get; private set; }

    public CashierForm()
    {
        Text = "MARSA KASIR";
        WindowState = FormWindowState.Maximized;
        MinimumSize = new Size(1000, 600);
        KeyPreview = true;

        BuildCartGrid();
        BuildSuggestionGrid();

        var cap = new Label
        {
            Text = "Scan barcode atau ketik nama produk   (3*kode = jumlah 3)",
            Dock = DockStyle.Top, Height = 24, ForeColor = Muted
        };
        searchBox.Controls.Add(txtBarcode);
        searchBox.Controls.Add(cap);

        leftPane.Controls.Add(grid);
        leftPane.Controls.Add(lblEmpty);
        leftPane.Controls.Add(searchBox);
        leftPane.Controls.Add(sug);

        // urutan Add penting: Fill dulu, tepi (Top/Right) belakangan
        Controls.Add(leftPane);
        Controls.Add(BuildRightPanel());
        Controls.Add(BuildHeader());

        txtBarcode.TextChanged += (_, _) => { searchTimer.Stop(); searchTimer.Start(); };
        searchTimer.Tick += (_, _) => { searchTimer.Stop(); RefreshSuggestions(); };
        txtBarcode.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; OnEnter(); }
        };
        leftPane.Resize += (_, _) => { if (sug.Visible) PositionSug(sug.Rows.Count); };
        KeyDown += OnKey;
        FormClosed += (_, _) => searchTimer.Stop();
        Shown += (_, _) => { UpdateTotal(); txtBarcode.Focus(); };
    }

    // =================== tampilan ===================

    void BuildCartGrid()
    {
        grid.BorderStyle = BorderStyle.None;
        grid.BackgroundColor = Color.White;
        grid.EnableHeadersVisualStyles = false;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        var hs = grid.ColumnHeadersDefaultCellStyle;
        hs.BackColor = hs.SelectionBackColor = Soft;
        hs.ForeColor = hs.SelectionForeColor = Muted;
        hs.Font = new Font("Segoe UI", 10, FontStyle.Bold);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 40;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.GridColor = LineColor;
        grid.AllowUserToResizeRows = false;
        grid.RowTemplate.Height = 46;
        var ds = grid.DefaultCellStyle;
        ds.Font = new Font("Segoe UI", 13);
        ds.ForeColor = Ink;
        ds.SelectionBackColor = Pick;
        ds.SelectionForeColor = Ink;
        ds.Padding = new Padding(6, 0, 6, 0);

        var L = DataGridViewContentAlignment.MiddleLeft;
        var R = DataGridViewContentAlignment.MiddleRight;
        var C = DataGridViewContentAlignment.MiddleCenter;
        void Col(string title, string prop, int w, DataGridViewContentAlignment align,
            bool fill = false, bool bold = false, bool num = false)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            c.DefaultCellStyle.Alignment = align;
            c.HeaderCell.Style.Alignment = align;
            if (num) c.DefaultCellStyle.Format = "N0";
            if (bold) c.DefaultCellStyle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns.Add(c);
        }
        Col("No", "", 60, C);
        Col("Produk", "Name", 300, L, fill: true);
        Col("Harga", "SellPrice", 140, R, num: true);
        Col("Qty", "Qty", 90, C);
        Col("Subtotal", "Subtotal", 170, R, bold: true, num: true);
        grid.DataSource = cart;

        grid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex == 0) { e.Value = (e.RowIndex + 1).ToString(); e.FormattingApplied = true; }
        };
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) EditQty(); };
        grid.MouseDown += (_, _) => HideSug();
    }

    void BuildSuggestionGrid()
    {
        sug.Visible = false;
        sug.BorderStyle = BorderStyle.FixedSingle;
        sug.BackgroundColor = Color.White;
        sug.ColumnHeadersVisible = false;
        sug.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        sug.GridColor = LineColor;
        sug.AllowUserToResizeRows = false;
        sug.RowTemplate.Height = 42;
        sug.ScrollBars = ScrollBars.Vertical;
        var ds = sug.DefaultCellStyle;
        ds.Font = new Font("Segoe UI", 12);
        ds.ForeColor = Ink;
        ds.SelectionBackColor = Pick;
        ds.SelectionForeColor = Ink;
        ds.Padding = new Padding(8, 0, 8, 0);

        sug.Columns.Add(new DataGridViewTextBoxColumn
            { DataPropertyName = "Name", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        var price = new DataGridViewTextBoxColumn { DataPropertyName = "SellPrice", Width = 130 };
        price.DefaultCellStyle.Format = "N0";
        price.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        sug.Columns.Add(price);
        var stock = new DataGridViewTextBoxColumn { DataPropertyName = "Stock", Width = 110 };
        stock.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        sug.Columns.Add(stock);

        sug.CellFormatting += (_, e) =>
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 2 || sug.Rows[e.RowIndex].DataBoundItem is not Product p) return;
            e.Value = $"stok {p.Stock}";
            e.CellStyle.ForeColor = p.Stock <= 0 ? Red : Muted;
            e.FormattingApplied = true;
        };
        sug.CellClick += (_, e) => { if (e.RowIndex >= 0) ChooseSuggestion(); };
    }

    static Button Btn(string text, Color back, Color fore, float size = 11, bool bold = false)
    {
        var b = new Button
        {
            Text = text, Dock = DockStyle.Fill, FlatStyle = FlatStyle.Flat, BackColor = back, ForeColor = fore,
            Font = new Font("Segoe UI", size, bold ? FontStyle.Bold : FontStyle.Regular),
            Cursor = Cursors.Hand, TabStop = false, Tag = "keep", UseVisualStyleBackColor = false,
            Margin = new Padding(4)
        };
        b.FlatAppearance.BorderSize = 0;
        return b;
    }

    Panel BuildHeader()
    {
        var header = new Panel { Dock = DockStyle.Top, Height = 56, BackColor = Navy };
        var lblStore = new Label
        {
            Text = SettingsRepository.Get("store_name", "MARSA KASIR"), Dock = DockStyle.Left, Width = 480,
            ForeColor = Color.White, Font = new Font("Segoe UI", 16, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(20, 0, 0, 0)
        };
        var lblClock = new Label
        {
            Dock = DockStyle.Right, Width = 340, ForeColor = Color.FromArgb(203, 213, 225),
            Font = new Font("Segoe UI", 11), TextAlign = ContentAlignment.MiddleRight,
            Padding = new Padding(0, 0, 14, 0)
        };
        var btnMenu = new Button
        {
            Text = "☰  Menu", Dock = DockStyle.Right, Width = 120, FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(51, 65, 85), ForeColor = Color.White,
            Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand,
            TabStop = false, Tag = "keep", UseVisualStyleBackColor = false
        };
        btnMenu.FlatAppearance.BorderSize = 0;

        var menu = new ContextMenuStrip { Font = new Font("Segoe UI", 11), ShowImageMargin = false };
        void Item(string text, Keys key, bool adminOnly = false)
        {
            if (adminOnly && !Session.IsAdmin) return;
            menu.Items.Add(text, null, (_, _) => { OnKey(null, new KeyEventArgs(key)); txtBarcode.Focus(); });
        }
        Item("Riwayat transaksi      F11", Keys.F11);
        Item("Laporan      F7", Keys.F7, true);
        Item("Kelola produk      F10", Keys.F10, true);
        Item("Pengaturan      F12", Keys.F12, true);
        Item("Pengguna      F6", Keys.F6, true);
        Item("Log aktivitas      F5", Keys.F5, true);
        Item("Backup & pulihkan      F1", Keys.F1, true);
        menu.Items.Add(new ToolStripSeparator());
        Item("Ganti user / keluar      F4", Keys.F4);
        btnMenu.Click += (_, _) => menu.Show(btnMenu, new Point(0, btnMenu.Height));

        void Tick() => lblClock.Text =
            $"{Session.Current?.Username} ({Session.Current?.Role})     {DateTime.Now:dd/MM/yyyy HH:mm:ss}";
        var clock = new System.Windows.Forms.Timer { Interval = 1000 };
        clock.Tick += (_, _) => Tick();
        Tick();
        clock.Start();
        FormClosed += (_, _) => clock.Stop();

        header.Controls.Add(lblClock);
        header.Controls.Add(btnMenu);
        header.Controls.Add(lblStore);
        return header;
    }

    Panel BuildRightPanel()
    {
        var right = new Panel
            { Dock = DockStyle.Right, Width = 400, BackColor = Color.White, Padding = new Padding(22) };
        var rp = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 6 };
        rp.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        rp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rp.RowStyles.Add(new RowStyle(SizeType.Absolute, 74));
        rp.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        rp.RowStyles.Add(new RowStyle(SizeType.Absolute, 84));
        rp.RowStyles.Add(new RowStyle(SizeType.Absolute, 130));
        rp.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var caption = new Label
        {
            Text = "TOTAL BELANJA", AutoSize = true, ForeColor = Muted,
            Font = new Font("Segoe UI", 10, FontStyle.Bold)
        };

        var btnPay = Btn("BAYAR     F8", Green, Color.White, 18, bold: true);
        btnPay.Margin = new Padding(0, 12, 0, 8);
        btnPay.Click += (_, _) => Pay();

        var actions = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actions.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        actions.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        Button Act(string text, Keys key, Color back, Color fore)
        {
            var b = Btn(text, back, fore, 10.5f);
            b.Click += (_, _) => { OnKey(null, new KeyEventArgs(key)); txtBarcode.Focus(); };
            return b;
        }
        actions.Controls.Add(Act("Daftar produk\nF2", Keys.F2, Soft, Ink), 0, 0);
        actions.Controls.Add(Act("Kasbon\nCtrl+K", Keys.Control | Keys.K, Soft, Ink), 1, 0);
        actions.Controls.Add(Act("Buka laci\nF3", Keys.F3, Soft, Ink), 0, 1);
        actions.Controls.Add(Act("Kosongkan\nF9", Keys.F9, Color.FromArgb(254, 242, 242), Red), 1, 1);

        rp.Controls.Add(caption, 0, 0);
        rp.Controls.Add(lblTotal, 0, 1);
        rp.Controls.Add(lblItems, 0, 2);
        rp.Controls.Add(btnPay, 0, 3);
        rp.Controls.Add(actions, 0, 4);
        rp.Controls.Add(lblStatus, 0, 5);
        right.Controls.Add(rp);
        return right;
    }

    // =================== tombol keyboard ===================

    void OnKey(object? s, KeyEventArgs e)
    {
        if (e.KeyCode >= Keys.F1 && e.KeyCode <= Keys.F12) HideSug();
        switch (e.KeyCode)
        {
            case Keys.F1 when Session.IsAdmin:
                using (var bk = new BackupForm()) bk.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F2: OpenPicker(); e.Handled = true; break;
            case Keys.F3: OpenDrawerManual(); e.Handled = true; break;
            case Keys.F4: Logout(); e.Handled = true; break;
            case Keys.F5 when Session.IsAdmin:
                using (var lg = new ActivityLogForm()) lg.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F6 when Session.IsAdmin:
                using (var us = new UserForm()) us.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F7 when Session.IsAdmin:
                using (var rf = new ReportForm()) rf.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F8: Pay(); e.Handled = true; break;
            case Keys.F9: ClearCart(); e.Handled = true; break;
            case Keys.F10 when Session.IsAdmin:
                using (var pf = new ProductForm()) pf.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F11:
                using (var hf = new SalesHistoryForm()) hf.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.F12 when Session.IsAdmin:
                using (var sf = new SettingsForm()) sf.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; break;
            case Keys.K when e.Control:
                using (var kb = new KasbonForm()) kb.ShowDialog(this);
                txtBarcode.Focus(); e.Handled = true; e.SuppressKeyPress = true; break;
            case Keys.Add: ChangeQty(+1); e.SuppressKeyPress = true; break;
            case Keys.Subtract: ChangeQty(-1); e.SuppressKeyPress = true; break;
            case Keys.Up:
                if (sug.Visible) MoveSug(-1); else SelectRow((grid.CurrentRow?.Index ?? 0) - 1);
                e.Handled = true; break;
            case Keys.Down:
                if (sug.Visible) MoveSug(+1); else SelectRow((grid.CurrentRow?.Index ?? 0) + 1);
                e.Handled = true; break;
            case Keys.Escape:
                if (sug.Visible) HideSug(); else txtBarcode.Clear();
                e.Handled = true; break;
            case Keys.Delete when txtBarcode.Text == "": RemoveRow(); e.Handled = true; break;
        }
    }

    // =================== pencarian & input ===================

    // "3*sabun" -> qty 3, term "sabun"
    void ParseInput(out string term, out int qty)
    {
        term = txtBarcode.Text.Trim();
        qty = 1;
        var star = term.IndexOf('*');
        if (star > 0 && int.TryParse(term[..star], out var q) && q > 0)
        {
            qty = q;
            term = term[(star + 1)..].Trim();
        }
    }

    void OnEnter()
    {
        ParseInput(out var term, out var qty);
        if (term == "") return;

        // 1) barcode cocok persis (scanner): langsung masuk keranjang
        var exact = ProductRepository.GetByBarcode(term);
        if (exact != null)
        {
            searchTimer.Stop();
            HideSug();
            AddProduct(exact, qty);
            Status($"{exact.Name}  x{qty}");
            txtBarcode.Clear();
            return;
        }

        // 2) daftar saran sedang tampil: ambil yang disorot, tanya jumlahnya
        if (sug.Visible && sug.CurrentRow?.DataBoundItem is Product p)
        {
            HideSug();
            AskQtyAndAdd(p, qty);
            return;
        }

        // 3) saran belum tampil: tampilkan dulu, Enter berikutnya memilih
        searchTimer.Stop();
        RefreshSuggestions();
        if (!sug.Visible)
        {
            Status($"Tidak ditemukan: {term}", error: true);
            SystemSounds.Beep.Play();
            txtBarcode.SelectAll();
        }
    }

    void RefreshSuggestions()
    {
        ParseInput(out var term, out _);
        if (term.Length == 0) { HideSug(); return; }
        var list = ProductRepository.Search(term);
        if (list.Count == 0)
        {
            HideSug();
            Status($"Tidak ada produk yang cocok: {term}");
            return;
        }
        sug.DataSource = list;
        sug.Visible = true;
        sug.BringToFront();
        PositionSug(list.Count);
        sug.CurrentCell = sug.Rows[0].Cells[0];
    }

    void PositionSug(int count)
    {
        sug.Left = leftPane.Padding.Left;
        sug.Top = searchBox.Top + txtBarcode.Bottom;
        sug.Width = leftPane.ClientSize.Width - leftPane.Padding.Horizontal;
        sug.Height = Math.Min(Math.Max(count, 1), 7) * sug.RowTemplate.Height + 2;
    }

    void HideSug() { if (sug.Visible) sug.Visible = false; }

    void MoveSug(int d)
    {
        if (sug.Rows.Count == 0) return;
        var i = Math.Clamp((sug.CurrentRow?.Index ?? -1) + d, 0, sug.Rows.Count - 1);
        sug.CurrentCell = sug.Rows[i].Cells[0];
    }

    void ChooseSuggestion()
    {
        if (sug.CurrentRow?.DataBoundItem is not Product p) return;
        ParseInput(out _, out var qty);
        HideSug();
        AskQtyAndAdd(p, qty);
    }

    // Form jumlah muncul untuk barang yang dipilih dari hasil pencarian
    void AskQtyAndAdd(Product p, int qty)
    {
        using var f = new QuantityForm(p.Name, p.SellPrice, qty, p.Stock);
        if (f.ShowDialog(this) == DialogResult.OK)
        {
            AddProduct(p, f.Qty);
            Status($"{p.Name}  x{f.Qty}");
            txtBarcode.Clear();
        }
        txtBarcode.Focus();
    }

    void OpenPicker()
    {
        using var pk = new ProductPickerForm();
        if (pk.ShowDialog(this) == DialogResult.OK && pk.Selected is { } p) AskQtyAndAdd(p, 1);
        txtBarcode.Focus();
    }

    // =================== keranjang ===================

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

    void EditQty()
    {
        var idx = grid.CurrentRow?.Index ?? -1;
        if (idx < 0) return;
        var it = cart[idx];
        using var f = new QuantityForm(it.Name, it.SellPrice, it.Qty, null, "Ubah Jumlah");
        if (f.ShowDialog(this) == DialogResult.OK && f.Qty != it.Qty)
        {
            if (f.Qty < it.Qty) ActivityLog.Write("Kurangi jumlah", $"{it.Name} {it.Qty} -> {f.Qty}");
            it.Qty = f.Qty;
            cart.ResetItem(idx);
            UpdateTotal();
        }
        txtBarcode.Focus();
    }

    void ChangeQty(int delta)
    {
        var idx = grid.CurrentRow?.Index ?? -1;
        if (idx < 0) return;
        cart[idx].Qty += delta;
        if (cart[idx].Qty <= 0) { cart[idx].Qty -= delta; RemoveRow(); return; }
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

    void UpdateTotal()
    {
        lblTotal.Text = Fmt.Rp(cart.Sum(x => x.Subtotal));
        lblItems.Text = cart.Count == 0
            ? "Belum ada barang"
            : $"{cart.Count} jenis   |   {cart.Sum(x => x.Qty)} barang";
        grid.Visible = cart.Count > 0;
        lblEmpty.Visible = cart.Count == 0;
    }

    void ClearCart()
    {
        if (cart.Count > 0 &&
            MessageBox.Show("Kosongkan keranjang?", "Konfirmasi", MessageBoxButtons.YesNo) == DialogResult.Yes)
        {
            ActivityLog.Write("Kosongkan keranjang", $"{cart.Count} baris, {Fmt.Rp(cart.Sum(x => x.Subtotal))}");
            cart.Clear();
            UpdateTotal();
        }
        txtBarcode.Focus();
    }

    // =================== bayar & lain-lain ===================

    void Pay()
    {
        HideSug();
        PayCore();
        txtBarcode.Focus();
    }

    void PayCore()
    {
        if (cart.Count == 0) return;
        long total = cart.Sum(x => x.Subtotal);
        using var f = new PaymentForm(total);
        if (f.ShowDialog(this) != DialogResult.OK) return;

        var id = SaleRepository.Save(cart, total, f.Paid, f.CustomerId);
        ActivityLog.Write("Transaksi", $"#{id} total {Fmt.Rp(total)}"
            + (f.CustomerId != null ? $" (kasbon {Fmt.Rp(total - f.Paid)})" : ""));
        if (f.Paid > 0 && SettingsRepository.Get("drawer_auto", "0") == "1") CashDrawer.TryOpen();
        if (SettingsRepository.Get("auto_print", "0") == "1") ReceiptPrinter.TryPrint(id);

        cart.Clear();
        UpdateTotal();
        lblStatus.Font = DoneFont;
        lblStatus.ForeColor = Green;
        lblStatus.Text = f.CustomerId != null
            ? $"Transaksi #{id} selesai\nMasuk kasbon: {Fmt.Rp(total - f.Paid)}"
            : $"Transaksi #{id} selesai\nKembalian: {Fmt.Rp(f.Paid - total)}";
    }

    void Status(string text, bool error = false)
    {
        lblStatus.Font = StatusFont;
        lblStatus.ForeColor = error ? Red : Ink;
        lblStatus.Text = text;
    }

    void OpenDrawerManual()
    {
        CashDrawer.TryOpen();
        ActivityLog.Write("Buka laci manual");
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
}
