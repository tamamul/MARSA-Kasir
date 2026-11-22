using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class ProductEditForm : BaseForm
{
    readonly Product p;
    readonly TextBox txtBarcode = new() { Dock = DockStyle.Fill };
    readonly TextBox txtName = new() { Dock = DockStyle.Fill };
    readonly TextBox txtUnit = new() { Dock = DockStyle.Fill };
    readonly NumericUpDown numBuy = Num(), numSell = Num(), numStock = Num(), numMin = Num();

    static NumericUpDown Num() => new()
        { Maximum = 999_999_999, ThousandsSeparator = true, Dock = DockStyle.Fill };

    public ProductEditForm(Product product)
    {
        p = product;
        Text = p.Id == 0 ? "Tambah Produk" : "Ubah Produk";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(380, 300);

        txtBarcode.Text = p.Barcode;
        txtName.Text = p.Name;
        txtUnit.Text = p.Unit;
        numBuy.Value = p.BuyPrice;
        numSell.Value = p.SellPrice;
        numStock.Value = p.Stock;
        numMin.Value = p.MinStock;

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(string label, Control c)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            t.Controls.Add(c);
        }
        Row("Barcode", txtBarcode);
        Row("Nama", txtName);
        Row("Harga beli", numBuy);
        Row("Harga jual", numSell);
        Row("Stok", numStock);
        Row("Stok minimum", numMin);
        Row("Satuan", txtUnit);

        var btnSave = new Button { Text = "Simpan", Width = 90 };
        var btnCancel = new Button { Text = "Batal", Width = 90, DialogResult = DialogResult.Cancel };
        btnSave.Click += (_, _) => Save();
        var bar = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 44, Padding = new Padding(8) };
        bar.Controls.Add(btnCancel);
        bar.Controls.Add(btnSave);

        Controls.Add(t);
        Controls.Add(bar);
        AcceptButton = btnSave;
        CancelButton = btnCancel;
    }

    void Save()
    {
        if (txtBarcode.Text.Trim() == "" || txtName.Text.Trim() == "")
        {
            MessageBox.Show("Barcode dan nama wajib diisi.");
            return;
        }
        p.Barcode = txtBarcode.Text.Trim();
        p.Name = txtName.Text.Trim();
        p.Unit = txtUnit.Text.Trim() == "" ? "pcs" : txtUnit.Text.Trim();
        p.BuyPrice = (long)numBuy.Value;
        p.SellPrice = (long)numSell.Value;
        p.Stock = (int)numStock.Value;
        p.MinStock = (int)numMin.Value;
        DialogResult = DialogResult.OK;
    }
}
