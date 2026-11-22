using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class KasbonForm : BaseForm
{
    readonly TextBox txtSearch = new() { PlaceholderText = "Cari nama pelanggan...", Dock = DockStyle.Fill };
    readonly Label lblTotal = new()
    {
        AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold), ForeColor = Theme.Danger,
        Anchor = AnchorStyles.Right, Margin = new Padding(12, 6, 6, 0)
    };
    readonly DataGridView gridCust = MakeGrid(), gridLedger = MakeGrid();

    static DataGridView MakeGrid() => new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, MultiSelect = false, RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    static void Col(DataGridView g, string title, string prop, int w, bool num = false, bool fill = false)
    {
        var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
        if (num)
        {
            c.DefaultCellStyle.Format = "N0";
            c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
        }
        if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
        g.Columns.Add(c);
    }

    public KasbonForm()
    {
        Text = "Kasbon Pelanggan";
        Size = new Size(920, 640);
        StartPosition = FormStartPosition.CenterParent;

        Col(gridCust, "Nama", "Name", 240, fill: true);
        Col(gridCust, "Keterangan", "Note", 150);
        Col(gridCust, "Batas", "CreditLimit", 120, num: true);
        Col(gridCust, "Sisa Kasbon", "Balance", 150, num: true);

        Col(gridLedger, "Waktu", "Time", 160);
        Col(gridLedger, "Keterangan", "Info", 300, fill: true);
        Col(gridLedger, "Kasbon", "Kasbon", 130, num: true);
        Col(gridLedger, "Bayar", "Bayar", 130, num: true);

        var top = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        top.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        top.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        top.Controls.Add(txtSearch);
        top.Controls.Add(lblTotal);

        var btnPay = new Button { Text = "Terima pembayaran", AutoSize = true };
        var btnAdd = new Button { Text = "Tambah pelanggan", AutoSize = true };
        var btnEdit = new Button { Text = "Ubah", AutoSize = true };
        btnPay.Click += (_, _) => Receive();
        btnAdd.Click += (_, _) => Add();
        btnEdit.Click += (_, _) => Edit();
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(4) };
        bar.Controls.Add(btnPay);
        bar.Controls.Add(btnAdd);
        if (Session.IsAdmin) bar.Controls.Add(btnEdit);   // ubah batas hanya admin

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 4, ColumnCount = 1, Padding = new Padding(8) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(top, 0, 0);
        root.Controls.Add(gridCust, 0, 1);
        root.Controls.Add(gridLedger, 0, 2);
        root.Controls.Add(bar, 0, 3);
        Controls.Add(root);

        gridCust.CellFormatting += (_, e) =>
        {
            if (e.RowIndex >= 0 && gridCust.Columns[e.ColumnIndex].DataPropertyName == "Balance"
                && gridCust.Rows[e.RowIndex].DataBoundItem is Customer c && c.Balance > 0)
                e.CellStyle.ForeColor = Theme.Danger;
        };
        gridCust.SelectionChanged += (_, _) => ShowLedger();
        gridCust.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Receive(); };
        txtSearch.TextChanged += (_, _) => Reload();
        Shown += (_, _) => Reload();
    }

    Customer? Current => gridCust.CurrentRow?.DataBoundItem as Customer;

    void Reload(long? keep = null)
    {
        var list = CreditRepository.Customers(txtSearch.Text.Trim());
        gridCust.DataSource = list;
        if (keep != null)
            for (int i = 0; i < list.Count; i++)
                if (list[i].Id == keep) { gridCust.CurrentCell = gridCust.Rows[i].Cells[0]; break; }
        lblTotal.Text = "Total kasbon beredar: " + Fmt.Rp(CreditRepository.Customers().Sum(x => x.Balance));
        ShowLedger();
    }

    void ShowLedger() =>
        gridLedger.DataSource = Current is { } c ? CreditRepository.History(c.Id) : new List<LedgerRow>();

    void Add()
    {
        var c = new Customer();
        using var f = new CustomerEditForm(c);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        var id = CreditRepository.SaveCustomer(c);
        ActivityLog.Write("Tambah pelanggan", c.Name);
        Reload(id);
    }

    void Edit()
    {
        if (Current is not { } c) return;
        using var f = new CustomerEditForm(c);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        CreditRepository.SaveCustomer(c);
        ActivityLog.Write("Ubah pelanggan", c.Name);
        Reload(c.Id);
    }

    void Receive()
    {
        if (Current is not { } c) return;
        if (c.Balance <= 0) { MessageBox.Show("Pelanggan ini tidak punya sisa kasbon."); return; }
        using var f = new DebtPaymentForm(c);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        CreditRepository.AddPayment(c.Id, f.Amount, f.Note);
        ActivityLog.Write("Terima bayar kasbon", $"{c.Name} {Fmt.Rp(f.Amount)}");
        Reload(c.Id);
    }
}
