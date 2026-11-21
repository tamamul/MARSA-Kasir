using Microsoft.Data.Sqlite;
using MarsaKasir.Data;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class ProductForm : Form
{
    readonly TextBox txtSearch = new() { PlaceholderText = "Cari nama / barcode...", Dock = DockStyle.Fill };
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };

    public ProductForm()
    {
        Text = "Kelola Produk";
        Size = new Size(1000, 600);
        StartPosition = FormStartPosition.CenterScreen;

        void Col(string title, string prop, int w, string? fmt = null)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            if (fmt != null) c.DefaultCellStyle.Format = fmt;
            grid.Columns.Add(c);
        }
        Col("Barcode", "Barcode", 140);
        Col("Nama", "Name", 280);
        Col("Harga Beli", "BuyPrice", 110, "N0");
        Col("Harga Jual", "SellPrice", 110, "N0");
        Col("Stok", "Stock", 70);
        Col("Min", "MinStock", 60);
        Col("Satuan", "Unit", 70);

        var btnAdd = new Button { Text = "Tambah", Width = 90 };
        var btnEdit = new Button { Text = "Ubah", Width = 90 };
        var btnDel = new Button { Text = "Hapus", Width = 90 };
        var bar = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true };
        bar.Controls.AddRange(new Control[] { btnAdd, btnEdit, btnDel });

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, RowCount = 3, ColumnCount = 1 };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.Controls.Add(txtSearch, 0, 0);
        root.Controls.Add(grid, 0, 1);
        root.Controls.Add(bar, 0, 2);
        Controls.Add(root);

        btnAdd.Click += (_, _) => Edit(new Product());
        btnEdit.Click += (_, _) => { if (Current is { } p) Edit(p); };
        btnDel.Click += (_, _) => Remove();
        grid.CellDoubleClick += (_, e) => { if (e.RowIndex >= 0 && Current is { } p) Edit(p); };
        txtSearch.TextChanged += (_, _) => Reload();
        Shown += (_, _) => Reload();
    }

    Product? Current => grid.CurrentRow?.DataBoundItem as Product;

    void Reload() => grid.DataSource = ProductRepository.Search(txtSearch.Text.Trim());

    void Edit(Product p)
    {
        using var f = new ProductEditForm(p);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        try { ProductRepository.Save(p); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Barcode sudah dipakai produk lain.");
        }
        Reload();
    }

    void Remove()
    {
        if (Current is not { } p) return;
        if (MessageBox.Show($"Hapus \"{p.Name}\"?", "Konfirmasi", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        ProductRepository.Deactivate(p.Id);
        Reload();
    }
}