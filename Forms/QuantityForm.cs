using MarsaKasir.Helpers;
namespace MarsaKasir.Forms;

public class QuantityForm : BaseForm
{
    public int Qty { get; private set; }
    readonly long price;
    readonly int? stock;
    readonly TextBox txt = new()
    {
        Font = new Font("Segoe UI", 28, FontStyle.Bold), TextAlign = HorizontalAlignment.Center,
        Dock = DockStyle.Fill
    };
    readonly Label lblSub = new()
    {
        Dock = DockStyle.Top, Height = 40, Font = new Font("Segoe UI", 14, FontStyle.Bold),
        TextAlign = ContentAlignment.MiddleRight
    };
    readonly Label lblWarn = new() { Dock = DockStyle.Top, Height = 26, ForeColor = Color.DarkOrange };

    public QuantityForm(string name, long price, int qty, int? stock = null, string title = "Jumlah")
    {
        this.price = price;
        this.stock = stock;
        Text = title;
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(420, 330);
        Padding = new Padding(16);
        KeyPreview = true;

        var lblName = new Label
            { Text = name, Dock = DockStyle.Top, Height = 56, Font = new Font("Segoe UI", 13, FontStyle.Bold) };
        var lblInfo = new Label
        {
            Text = Fmt.Rp(price) + (stock != null ? $"   |   Stok: {stock}" : ""),
            Dock = DockStyle.Top, Height = 30, ForeColor = Color.Gray
        };

        var btnMinus = new Button
            { Text = "−", Dock = DockStyle.Left, Width = 72, Font = new Font("Segoe UI", 20), TabStop = false };
        var btnPlus = new Button
            { Text = "+", Dock = DockStyle.Right, Width = 72, Font = new Font("Segoe UI", 20), TabStop = false };
        var row = new Panel { Dock = DockStyle.Top, Height = 64 };
        row.Controls.Add(txt);
        row.Controls.Add(btnMinus);
        row.Controls.Add(btnPlus);

        var btnOk = new Button { Text = "Tambahkan (Enter)", Dock = DockStyle.Bottom, Height = 48 };

        txt.Text = Math.Clamp(qty, 1, 9999).ToString();
        txt.KeyPress += (_, e) =>
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar)) e.Handled = true;
        };
        txt.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Up) { Step(+1); e.Handled = true; }
            else if (e.KeyCode == Keys.Down) { Step(-1); e.Handled = true; }
        };
        txt.TextChanged += (_, _) => UpdateInfo();
        btnMinus.Click += (_, _) => Step(-1);
        btnPlus.Click += (_, _) => Step(+1);
        btnOk.Click += (_, _) => Confirm();
        KeyDown += (_, e) => { if (e.KeyCode == Keys.Escape) DialogResult = DialogResult.Cancel; };

        Controls.Add(btnOk);
        Controls.Add(lblWarn);
        Controls.Add(lblSub);
        Controls.Add(row);
        Controls.Add(lblInfo);
        Controls.Add(lblName);
        AcceptButton = btnOk;
        Shown += (_, _) => { txt.Focus(); txt.SelectAll(); };
        UpdateInfo();
    }

    int Current => int.TryParse(txt.Text, out var v) ? Math.Clamp(v, 0, 9999) : 0;

    void Step(int d)
    {
        txt.Text = Math.Clamp(Current + d, 1, 9999).ToString();
        txt.SelectAll();
    }

    void UpdateInfo()
    {
        lblSub.Text = "Subtotal: " + Fmt.Rp(price * Current);
        lblWarn.Text = stock != null && Current > stock ? $"Jumlah melebihi stok ({stock})" : "";
    }

    void Confirm()
    {
        if (Current < 1) { MessageBox.Show("Jumlah minimal 1."); return; }
        Qty = Current;
        DialogResult = DialogResult.OK;
    }
}
