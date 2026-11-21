using Microsoft.Data.Sqlite;
using MarsaKasir.Data;
using MarsaKasir.Helpers;
using MarsaKasir.Models;
namespace MarsaKasir.Forms;

public class UserForm : Form
{
    readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
        AllowUserToAddRows = false, MultiSelect = false, RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    readonly TextBox txtUser = new() { PlaceholderText = "Username", Width = 140 };
    readonly TextBox txtPass = new() { PlaceholderText = "Password", Width = 140, UseSystemPasswordChar = true };
    readonly ComboBox cmbRole = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 80 };

    public UserForm()
    {
        Text = "Pengguna";
        Size = new Size(720, 480);
        StartPosition = FormStartPosition.CenterParent;

        void Col(string title, string prop, int w)
            => grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w });
        Col("Username", "Username", 220);
        Col("Peran", "Role", 120);
        Col("Status", "Status", 120);

        cmbRole.Items.AddRange(new object[] { "kasir", "admin" });
        cmbRole.SelectedIndex = 0;

        var btnAdd = new Button { Text = "Tambah", AutoSize = true };
        var btnReset = new Button { Text = "Reset password", AutoSize = true };
        var btnToggle = new Button { Text = "Aktif / Nonaktif", AutoSize = true };
        btnAdd.Click += (_, _) => Add();
        btnReset.Click += (_, _) => Reset();
        btnToggle.Click += (_, _) => Toggle();

        var bar = new FlowLayoutPanel { Dock = DockStyle.Bottom, AutoSize = true, Padding = new Padding(6) };
        bar.Controls.AddRange(new Control[] { txtUser, txtPass, cmbRole, btnAdd, btnReset, btnToggle });

        Controls.Add(grid);
        Controls.Add(bar);
        Shown += (_, _) => Reload();
    }

    User? Current => grid.CurrentRow?.DataBoundItem as User;
    void Reload() => grid.DataSource = UserRepository.List();

    void Add()
    {
        var name = txtUser.Text.Trim();
        if (name == "" || txtPass.Text.Length < 4)
        {
            MessageBox.Show("Username wajib diisi, password minimal 4 karakter.");
            return;
        }
        try { UserRepository.Create(name, txtPass.Text, cmbRole.Text); }
        catch (SqliteException ex) when (ex.SqliteErrorCode == 19)
        {
            MessageBox.Show("Username sudah dipakai.");
            return;
        }
        ActivityLog.Write("Tambah pengguna", $"{name} ({cmbRole.Text})");
        txtUser.Clear();
        txtPass.Clear();
        Reload();
    }

    void Reset()
    {
        if (Current is not { } u) return;
        if (txtPass.Text.Length < 4)
        {
            MessageBox.Show("Ketik password baru (minimal 4 karakter) di kolom Password.");
            return;
        }
        UserRepository.ResetPassword(u.Id, txtPass.Text);
        ActivityLog.Write("Reset password", u.Username);
        txtPass.Clear();
        MessageBox.Show($"Password {u.Username} diganti.");
    }

    void Toggle()
    {
        if (Current is not { } u) return;
        if (u.Id == Session.Current?.Id)
        {
            MessageBox.Show("Tidak bisa menonaktifkan akun sendiri.");
            return;
        }
        UserRepository.SetActive(u.Id, !u.Active);
        ActivityLog.Write(u.Active ? "Nonaktifkan pengguna" : "Aktifkan pengguna", u.Username);
        Reload();
    }
}