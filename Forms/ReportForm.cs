using MarsaKasir.Data;
using MarsaKasir.Helpers;
namespace MarsaKasir.Forms;

public class ReportForm : Form
{
    readonly DateTimePicker dtFrom = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly DateTimePicker dtTo = new() { Format = DateTimePickerFormat.Short, Width = 110 };
    readonly ComboBox cmbGroup = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    readonly Label lblSummary = new()
    {
        Dock = DockStyle.Fill, Font = new Font("Segoe UI", 15, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleLeft
    };
    readonly DataGridView gridPeriod = MakeGrid(), gridTop = MakeGrid(), gridLow = MakeGrid();

    static DataGridView MakeGrid() => new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, RowHeadersVisible = false
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

    public ReportForm()
    {
        Text = "Laporan";
        Size = new Size(900, 600);
        StartPosition = FormStartPosition.CenterParent;

        Col(gridPeriod, "Periode", "Period", 140);
        Col(gridPeriod, "Transaksi", "Trx", 100, num: true);
        Col(gridPeriod, "Omzet", "Omzet", 160, num: true);
        Col(gridPeriod, "Keuntungan", "Profit", 160, num: true);

        Col(gridTop, "Produk", "Name", 300, fill: true);
        Col(gridTop, "Terjual", "Qty", 90, num: true);
        Col(gridTop, "Omzet", "Omzet", 150, num: true);
        Col(gridTop, "Keuntungan", "Profit", 150, num: true);

        Col(gridLow, "Produk", "Name", 300, fill: true);
        Col(gridLow, "Stok", "Stock", 90, num: true);
        Col(gridLow, "Minimum", "MinStock", 90, num: true);
        Col(gridLow, "Satuan", "Unit", 80);

        cmbGroup.Items.AddRange(new object[] { "Per Hari", "Per Bulan" });
        cmbGroup.SelectedIndex = 0;
        var now = DateTime.Today;
        dtFrom.Value = new DateTime(now.Year, now.Month, 1);
        dtTo.Value = now;

        var btnToday = new Button { Text = "Hari ini", AutoSize = true };
        var btnMonth = new Button { Text = "Bulan ini", AutoSize = true };
        var btnShow = new Button { Text = "Tampilkan", AutoSize = true };

        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(6) };
        bar.Controls.Add(new Label { Text = "Dari", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        bar.Controls.Add(dtFrom);
        bar.Controls.Add(new Label { Text = "Sampai", AutoSize = true, Margin = new Padding(3, 8, 3, 0) });
        bar.Controls.Add(dtTo);
        bar.Controls.AddRange(new Control[] { btnToday, btnMonth, cmbGroup, btnShow });

        var tabs = new TabControl { Dock = DockStyle.Fill };
        void Tab(string title, Control c)
        {
            var p = new TabPage(title);
            p.Controls.Add(c);
            tabs.TabPages.Add(p);
        }
        Tab("Penjualan", gridPeriod);
        Tab("Produk Terjual", gridTop);
        Tab("Stok Menipis", gridLow);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 56));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(bar, 0, 0);
        root.Controls.Add(lblSummary, 0, 1);
        root.Controls.Add(tabs, 0, 2);
        Controls.Add(root);

        btnShow.Click += (_, _) => Reload();
        cmbGroup.SelectedIndexChanged += (_, _) => Reload();
        btnToday.Click += (_, _) => { dtFrom.Value = dtTo.Value = DateTime.Today; Reload(); };
        btnMonth.Click += (_, _) =>
        {
            var d = DateTime.Today;
            dtFrom.Value = new DateTime(d.Year, d.Month, 1);
            dtTo.Value = d;
            Reload();
        };
        Shown += (_, _) => Reload();
    }

    void Reload()
    {
        var from = dtFrom.Value.ToString("yyyy-MM-dd");
        var to = dtTo.Value.ToString("yyyy-MM-dd");
        var s = ReportRepository.GetSummary(from, to);
        lblSummary.Text = $"Transaksi: {s.Trx}     Omzet: {Fmt.Rp(s.Omzet)}     Untung: {Fmt.Rp(s.Profit)}";
        gridPeriod.DataSource = ReportRepository.ByPeriod(from, to, cmbGroup.SelectedIndex == 1);
        gridTop.DataSource = ReportRepository.TopProducts(from, to);
        gridLow.DataSource = ReportRepository.LowStock();
    }
}