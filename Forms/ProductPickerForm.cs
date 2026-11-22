using MarsaKasir.Data;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class ProductPickerForm : BaseForm
{
    readonly TextBox txt = new() { Dock = DockStyle.Fill, Font = new Font("Segoe UI", 16) };
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, MultiSelect = false, RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, Font = new Font("Segoe UI", 12)
    };
    public Product? Selected { get; private set; }

    public ProductPickerForm(string initial = "")
    {
        Text = "Cari Produk";
        Size = new Size(760, 520);
        StartPosition = FormStartPosition.CenterParent;

        void Col(string title, string prop, int w, bool num = false, bool fill = false)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            if (num)
            {
                c.DefaultCellStyle.Format = "N0";
                c.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }
            if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns.Add(c);
        }
        Col("Nama", "Name", 300, fill: true);
        Col("Harga", "SellPrice", 130, num: true);
        Col("Stok", "Stock", 80, num: true);

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 2, ColumnCount = 1, Padding = new Padding(8) };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.Controls.Add(txt, 0, 0);
        root.Controls.Add(grid, 0, 1);
        Controls.Add(root);

        txt.Text = initial;
        txt.TextChanged += (_, _) => Reload();
        txt.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Down) { Move(+1); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Up) { Move(-1); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Enter) { Choose(); e.SuppressKeyPress = true; }
            else if (e.KeyCode == Keys.Escape) { DialogResult = DialogResult.Cancel; }
        };
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) Choose(); };
        Shown += (_, _) => { Reload(); txt.Focus(); txt.SelectionStart = txt.Text.Length; };
    }

    void Reload() => grid.DataSource = ProductRepository.Search(txt.Text.Trim());

    void Move(int d)
    {
        var i = (grid.CurrentRow?.Index ?? -1) + d;
        if (i >= 0 && i < grid.Rows.Count) grid.CurrentCell = grid.Rows[i].Cells[0];
    }

    void Choose()
    {
        if (grid.CurrentRow?.DataBoundItem is not Product p) return;
        Selected = p;
        DialogResult = DialogResult.OK;
    }
}
