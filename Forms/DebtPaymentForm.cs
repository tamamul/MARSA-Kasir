using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class DebtPaymentForm : BaseForm
{
    public long Amount { get; private set; }
    public string Note => txtNote.Text.Trim();
    readonly long balance;
    readonly NumericUpDown num = new()
        { Maximum = 999_999_999, ThousandsSeparator = true, Font = new Font("Segoe UI", 18), Dock = DockStyle.Top };
    readonly TextBox txtNote = new()
        { Dock = DockStyle.Top, PlaceholderText = "Catatan (mis. potong gaji bulan ini)" };

    public DebtPaymentForm(Customer c)
    {
        balance = c.Balance;
        Text = "Terima Pembayaran Kasbon";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(420, 240);
        Padding = new Padding(12);

        num.Value = c.Balance;   // bawaan: lunas
        var lbl = new Label
        {
            Text = $"{c}\nSisa kasbon: {Fmt.Rp(c.Balance)}", Dock = DockStyle.Top, Height = 56,
            Font = new Font("Segoe UI", 11, FontStyle.Bold)
        };
        var btnOk = new Button { Text = "Terima (Enter)", Dock = DockStyle.Bottom, Height = 44 };
        btnOk.Click += (_, _) => Confirm();

        Controls.Add(btnOk);
        Controls.Add(txtNote);
        Controls.Add(num);
        Controls.Add(lbl);
        AcceptButton = btnOk;
        Shown += (_, _) => { num.Focus(); num.Select(0, num.Text.Length); };
    }

    void Confirm()
    {
        var v = (long)num.Value;
        if (v <= 0 || v > balance)
        {
            MessageBox.Show($"Jumlah harus antara Rp 1 dan {Fmt.Rp(balance)}.");
            return;
        }
        Amount = v;
        DialogResult = DialogResult.OK;
    }
}
