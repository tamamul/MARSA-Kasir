using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class PaymentForm : BaseForm
{
    public long Paid { get; private set; }
    public long? CustomerId { get; private set; }   // terisi bila ada sisa yang masuk kasbon
    readonly long total;
    readonly TextBox txtPaid = new()
        { Font = new Font("Segoe UI", 22), Dock = DockStyle.Top, TextAlign = HorizontalAlignment.Right };
    readonly Label lblChange = new()
    {
        Font = new Font("Segoe UI", 18, FontStyle.Bold), Dock = DockStyle.Top, Height = 50,
        TextAlign = ContentAlignment.MiddleRight
    };
    readonly ComboBox cmbCust = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };

    public PaymentForm(long total)
    {
        this.total = total;
        Text = "Pembayaran";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(420, 320);

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

        var btnNew = new Button { Text = "+ Baru", Dock = DockStyle.Right, Width = 80, TabStop = false };
        btnNew.Click += (_, _) => NewCustomer();
        var custPanel = new Panel { Dock = DockStyle.Top, Height = 40 };
        custPanel.Controls.Add(cmbCust);
        custPanel.Controls.Add(btnNew);

        var btnOk = new Button { Text = "Bayar (Enter)", Dock = DockStyle.Bottom, Height = 44 };
        btnOk.Click += (_, _) => Confirm();

        // urutan Add: yang Dock Top ditumpuk dari bawah ke atas
        Controls.Add(btnOk);
        Controls.Add(lblChange);
        Controls.Add(quick);
        Controls.Add(txtPaid);
        Controls.Add(custPanel);
        Controls.Add(lblTotal);
        AcceptButton = btnOk;

        txtPaid.TextChanged += (_, _) => UpdateChange();
        cmbCust.SelectedIndexChanged += (_, _) => UpdateChange();
        LoadCustomers();
        Shown += (_, _) => { txtPaid.Focus(); UpdateChange(); };
    }

    Customer? Cust => cmbCust.SelectedItem as Customer;

    void LoadCustomers(long? select = null)
    {
        cmbCust.Items.Clear();
        cmbCust.Items.Add("Tunai (tanpa kasbon)");
        foreach (var c in CreditRepository.Customers()) cmbCust.Items.Add(c);
        cmbCust.SelectedIndex = 0;
        if (select != null)
            for (int i = 1; i < cmbCust.Items.Count; i++)
                if (((Customer)cmbCust.Items[i]!).Id == select) cmbCust.SelectedIndex = i;
    }

    void NewCustomer()
    {
        var c = new Customer();
        using var f = new CustomerEditForm(c);
        if (f.ShowDialog(this) != DialogResult.OK) return;
        var id = CreditRepository.SaveCustomer(c);
        ActivityLog.Write("Tambah pelanggan", c.Name);
        LoadCustomers(id);
        txtPaid.Focus();
    }

    long ParsePaid()
    {
        var digits = new string(txtPaid.Text.Where(char.IsDigit).ToArray());
        return long.TryParse(digits, out var v) ? v : 0;
    }

    void UpdateChange()
    {
        var diff = ParsePaid() - total;
        if (diff >= 0)
        {
            lblChange.Text = "Kembali: " + Fmt.Rp(diff);
            lblChange.ForeColor = Color.SeaGreen;
        }
        else if (Cust != null)
        {
            lblChange.Text = "Masuk kasbon: " + Fmt.Rp(-diff);
            lblChange.ForeColor = Color.DarkOrange;
        }
        else
        {
            lblChange.Text = "Kurang: " + Fmt.Rp(-diff);
            lblChange.ForeColor = Color.Firebrick;
        }
    }

    void Confirm()
    {
        var paid = ParsePaid();
        var cust = Cust;
        if (paid < total && cust == null)
        {
            MessageBox.Show("Uang kurang. Pilih pelanggan jika ingin dikasbon.");
            return;
        }
        if (cust != null && paid < total && cust.CreditLimit > 0
            && cust.Balance + (total - paid) > cust.CreditLimit)
        {
            MessageBox.Show($"Melebihi batas kasbon {Fmt.Rp(cust.CreditLimit)}.\nSisa kasbon saat ini {Fmt.Rp(cust.Balance)}.");
            return;
        }
        Paid = paid;
        CustomerId = paid < total ? cust?.Id : null;
        DialogResult = DialogResult.OK;
    }

    // Pembulatan ke atas ke pecahan uang yang umum: total 23.500 -> 25.000, 30.000, 50.000
    static IEnumerable<long> Suggest(long total) =>
        new long[] { 5000, 10000, 20000, 50000, 100000 }
            .Select(d => (total + d - 1) / d * d)
            .Where(v => v > total)
            .Distinct().OrderBy(v => v).Take(3);
}
