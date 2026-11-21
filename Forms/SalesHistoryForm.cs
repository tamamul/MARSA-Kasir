using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class SalesHistoryForm : Form
{
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, MultiSelect = false, RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    public SalesHistoryForm()
    {
        Text = "Riwayat Transaksi";
        Size = new Size(620, 500);
        StartPosition = FormStartPosition.CenterParent;

        void Col(string title, string prop, int w, string? fmt = null)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            if (fmt != null) c.DefaultCellStyle.Format = fmt;
            grid.Columns.Add(c);
        }
        Col("No", "Id", 80);
        Col("Waktu", "CreatedAt", 180, "dd/MM/yyyy HH:mm");
        Col("Total", "Total", 130, "N0");
        Col("Bayar", "Paid", 130, "N0");
        grid.DataSource = SaleRepository.Recent();

        var btn = new Button { Text = "Cetak ulang (Enter)", Dock = DockStyle.Bottom, Height = 44 };
        btn.Click += (_, _) => PrintCurrent();
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0) PrintCurrent(); };
        grid.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; PrintCurrent(); }
        };
        Controls.Add(grid);
        Controls.Add(btn);
    }

    void PrintCurrent()
    {
        if (grid.CurrentRow?.DataBoundItem is Sale s) ReceiptPrinter.TryPrint(s.Id);
    }
}