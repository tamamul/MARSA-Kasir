using MarsaKasir.Data;
namespace MarsaKasir.Forms;

public class BackupForm : BaseForm
{
    readonly TextBox txtFolder = new() { ReadOnly = true, Dock = DockStyle.Fill };
    readonly Label lblInfo = new() { AutoSize = true, Margin = new Padding(3, 12, 3, 12) };

    public BackupForm()
    {
        Text = "Backup & Pulihkan";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(580, 230);

        var btnPick = new Button { Text = "Pilih folder...", AutoSize = true };
        var btnNow = new Button { Text = "Backup sekarang", AutoSize = true };
        var btnRestore = new Button { Text = "Pulihkan dari file...", AutoSize = true };
        btnPick.Click += (_, _) => Pick();
        btnNow.Click += (_, _) => BackupNow();
        btnRestore.Click += (_, _) => Restore();

        var buttons = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        buttons.Controls.AddRange(new Control[] { btnNow, btnRestore });

        var t = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, Padding = new Padding(12) };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        void Full(Control c) { t.Controls.Add(c); t.SetColumnSpan(c, 2); }
        Full(new Label { Text = "Folder backup (disarankan flashdisk atau folder Google Drive):", AutoSize = true });
        t.Controls.Add(txtFolder);
        t.Controls.Add(btnPick);
        Full(lblInfo);
        Full(buttons);
        Controls.Add(t);
        ShowInfo();
    }

    void ShowInfo()
    {
        txtFolder.Text = BackupService.Folder;
        lblInfo.Text = "Backup terakhir: "
            + (BackupService.LastBackup()?.ToString("dd/MM/yyyy HH:mm") ?? "belum ada")
            + "\nOtomatis saat aplikasi dibuka dan ditutup (paling sering tiap 8 jam), 60 file terakhir disimpan.";
    }

    void Pick()
    {
        using var d = new FolderBrowserDialog { SelectedPath = BackupService.Folder };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        SettingsRepository.Set("backup_folder", d.SelectedPath);
        ShowInfo();
    }

    void BackupNow()
    {
        try
        {
            var file = BackupService.Run();
            ActivityLog.Write("Backup", Path.GetFileName(file));
            ShowInfo();
            MessageBox.Show("Backup tersimpan:\n" + file);
        }
        catch (Exception ex) { MessageBox.Show("Backup gagal: " + ex.Message); }
    }

    void Restore()
    {
        using var d = new OpenFileDialog { Filter = "Database MARSA KASIR (*.db)|*.db" };
        if (Directory.Exists(BackupService.Folder)) d.InitialDirectory = BackupService.Folder;
        if (d.ShowDialog(this) != DialogResult.OK) return;
        if (MessageBox.Show("Data saat ini akan diganti dengan isi file ini (data saat ini dicadangkan dulu). Lanjutkan?",
                "Pulihkan", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        try { BackupService.Restore(d.FileName); }
        catch (Exception ex) { MessageBox.Show("Gagal memulihkan (file bukan database MARSA KASIR?):\n" + ex.Message); return; }

        ActivityLog.Write("Pulihkan database", Path.GetFileName(d.FileName));
        MessageBox.Show("Database dipulihkan. Aplikasi akan dibuka ulang.");
        Application.Restart();
        Environment.Exit(0);
    }
}
