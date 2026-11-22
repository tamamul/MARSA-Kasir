using MarsaKasir.Helpers;
namespace MarsaKasir.Forms;

public class PaymentForm : BaseForm
{
    public long Paid { get; private set; }
    readonly long total;
    readonly TextBox txtPaid = new() { Font = new Font("Segoe UI", 22), Dock = DockStyle.Top, TextAlign = HorizontalAlignment.Right };
    readonly Label lblChange = new() { Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 50, TextAlign = ContentAlignment.MiddleRight };

    public PaymentForm(long total)
    {
        this.total = total;
        Text = "Pembayaran";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(420, 260);

        var lblTotal = new Label
        {
            Text = "Total: " + Fmt.Rp(total), Dock = DockStyle.Top, Height = 50,
            Font = new Font("Segoe UI", 18, FontStyle.Bold), TextAlign = ContentAlignment.MiddleRight
        };

        var quick = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 44, FlowDirection = FlowDirection.RightToLeft };
        var options = new List<(string, long)> { ("Uang pas", total) };
        options.AddRange(Suggest(total).Select(v => (Fmt.Num(v), v)));
        foreach (var (label, value) in options)
        {
            var b = new Button { Text = label, Width = 90, TabStop = false };
            b.Click += (_, _) => { txtPaid.Text = value.ToString(); txtPaid.Focus(); };
            quick.Controls.Add(b);
        }

        var btnOk = new Button { Text = "Bayar (Enter)", Dock = DockStyle.Bottom, Height = 44 };
        btnOk.Click += (_, _) => Confirm();

        // urutan Add: yang Dock Top ditumpuk dari bawah ke atas
        Controls.Add(btnOk);
        Controls.Add(lblChange);
        Controls.Add(quick);
        Controls.Add(txtPaid);
        Controls.Add(lblTotal);
        AcceptButton = btnOk;

        txtPaid.TextChanged += (_, _) => UpdateChange();
        Shown += (_, _) => { txtPaid.Focus(); UpdateChange(); };
    }

    long ParsePaid()
    {
        var digits = new string(txtPaid.Text.Where(char.IsDigit).ToArray());
        return long.TryParse(digits, out var v) ? v : 0;
    }

    void UpdateChange()
    {
        var diff = ParsePaid() - total;
        lblChange.Text = diff >= 0 ? "Kembali: " + Fmt.Rp(diff) : "Kurang: " + Fmt.Rp(-diff);
        lblChange.ForeColor = diff >= 0 ? Color.SeaGreen : Color.Firebrick;
    }

    void Confirm()
    {
        var paid = ParsePaid();
        if (paid < total) { MessageBox.Show("Uang kurang."); return; }
        Paid = paid;
        DialogResult = DialogResult.OK;
    }
    // Pembulatan ke atas ke pecahan uang yang umum: total 23.500 -> 25.000, 30.000, 50.000
    static IEnumerable<long> Suggest(long total) =>
        new long[] { 5000, 10000, 20000, 50000, 100000 }
            .Select(d => (total + d - 1) / d * d)
            .Where(v => v > total)
            .Distinct().OrderBy(v => v).Take(3);
}
