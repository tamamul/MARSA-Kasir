using MarsaKasir.Data;
namespace MarsaKasir.Forms;

public class ActivityLogForm : BaseForm
{
    public ActivityLogForm()
    {
        Text = "Log Aktivitas";
        Size = new Size(900, 520);
        StartPosition = FormStartPosition.CenterParent;

        var grid = new DataGridView
        {
            Dock = DockStyle.Fill, AutoGenerateColumns = false, ReadOnly = true,
            AllowUserToAddRows = false, RowHeadersVisible = false
        };
        void Col(string title, string prop, int w, bool fill = false)
        {
            var c = new DataGridViewTextBoxColumn { HeaderText = title, DataPropertyName = prop, Width = w };
            if (fill) c.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            grid.Columns.Add(c);
        }
        Col("Waktu", "Time", 150);
        Col("Pengguna", "User", 110);
        Col("Aksi", "Action", 170);
        Col("Detail", "Detail", 300, fill: true);
        grid.DataSource = ActivityLog.Recent();
        Controls.Add(grid);
    }
}
