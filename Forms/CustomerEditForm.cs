using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class CustomerEditForm : BaseForm
{
    readonly Customer c;
    readonly TextBox txtName = new() { Dock = DockStyle.Fill };
    readonly TextBox txtNote = new() { Dock = DockStyle.Fill, PlaceholderText = "mis. Guru / Karyawan" };
    readonly NumericUpDown numLimit = new()
        { Maximum = 999_999_999, ThousandsSeparator = true, Dock = DockStyle.Fill };

    public CustomerEditForm(Customer customer)
    {
        c = customer;
        Text = c.Id == 0 ? "Tambah Pelanggan" : "Ubah Pelanggan";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(400, 210);

        txtName.Text = c.Name;
        txtNote.Text = c.Note;
        numLimit.Value = c.CreditLimit;

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(string label, Control ctl)
        {
            t.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            t.Controls.Add(ctl);
        }
        Row("Nama", txtName);
        Row("Keterangan", txtNote);
        Row("Batas (0 = bebas)", numLimit);

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
        if (txtName.Text.Trim() == "") { MessageBox.Show("Nama wajib diisi."); return; }
        c.Name = txtName.Text.Trim();
        c.Note = txtNote.Text.Trim();
        c.CreditLimit = (long)numLimit.Value;
        DialogResult = DialogResult.OK;
    }
}
