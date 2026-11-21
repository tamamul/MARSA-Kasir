using MarsaKasir.Data;
using MarsaKasir.Helpers;
namespace MarsaKasir.Forms;

public class LoginForm : Form
{
    readonly TextBox txtUser = new() { Dock = DockStyle.Fill };
    readonly TextBox txtPass = new() { Dock = DockStyle.Fill, UseSystemPasswordChar = true };

    public LoginForm()
    {
        Text = "MARSA KASIR - Masuk";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(340, 170);

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(14) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 80));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.Controls.Add(new Label { Text = "Username", AutoSize = true, Anchor = AnchorStyles.Left });
        t.Controls.Add(txtUser);
        t.Controls.Add(new Label { Text = "Password", AutoSize = true, Anchor = AnchorStyles.Left });
        t.Controls.Add(txtPass);

        var btnOk = new Button { Text = "Masuk", Width = 90 };
        var btnExit = new Button { Text = "Keluar", Width = 90, DialogResult = DialogResult.Cancel };
        btnOk.Click += (_, _) => Login();
        var bar = new FlowLayoutPanel
            { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, Height = 44, Padding = new Padding(8) };
        bar.Controls.AddRange(new Control[] { btnExit, btnOk });

        Controls.Add(t);
        Controls.Add(bar);
        AcceptButton = btnOk;
        CancelButton = btnExit;
        Shown += (_, _) => txtUser.Focus();
    }

    void Login()
    {
        var name = txtUser.Text.Trim();
        var user = UserRepository.Authenticate(name, txtPass.Text);
        if (user == null)
        {
            ActivityLog.Write("Login gagal", "", name);
            MessageBox.Show("Username atau password salah.");
            txtPass.Clear();
            txtPass.Focus();
            return;
        }
        Session.Current = user;
        ActivityLog.Write("Login");
        if (user.Username.ToLower() == "admin" && txtPass.Text == "admin")
            MessageBox.Show("Password admin masih bawaan. Segera ganti lewat F6 (Pengguna).");
        DialogResult = DialogResult.OK;
    }
}