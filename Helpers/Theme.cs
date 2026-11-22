namespace MarsaKasir.Helpers;

public static class Theme
{
    public static readonly Color Primary = Color.FromArgb(25, 118, 210);
    public static readonly Color Success = Color.FromArgb(46, 125, 50);
    public static readonly Color Danger = Color.FromArgb(198, 40, 40);
    public static readonly Color Surface = Color.FromArgb(245, 247, 250);
    public static readonly Color Ink = Color.FromArgb(33, 37, 41);
    static readonly Font Base = new("Segoe UI", 10f);
    static readonly Font HeaderFont = new("Segoe UI", 10f, FontStyle.Bold);

    // Dipanggil sekali dari BaseForm; menelusuri semua kontrol di dalam form
    public static void Apply(Control root)
    {
        if (root is Form f) { f.Font = Base; f.BackColor = Surface; }
        foreach (Control c in root.Controls)
        {
            switch (c)
            {
                case DataGridView g: StyleGrid(g); break;
                case Button b: StyleButton(b); break;
            }
            Apply(c);
        }
        if (root is Form ff && ff.AcceptButton is Button ab) StyleButton(ab, primary: true);
    }

    public static void StyleButton(Button b, bool primary = false)
    {
        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.UseVisualStyleBackColor = false;
        b.BackColor = primary ? Primary : Color.FromArgb(224, 229, 236);
        b.ForeColor = primary ? Color.White : Ink;
        b.Cursor = Cursors.Hand;
        b.MinimumSize = new Size(0, 36);
    }

    public static void StyleGrid(DataGridView g, int rowHeight = 32)
    {
        var h = Math.Max(g.RowTemplate.Height, rowHeight);
        g.BorderStyle = BorderStyle.None;
        g.BackgroundColor = Color.White;
        g.EnableHeadersVisualStyles = false;
        g.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        g.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        g.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        g.ColumnHeadersDefaultCellStyle.Font = HeaderFont;
        g.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        g.ColumnHeadersHeight = 38;
        g.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        g.GridColor = Color.FromArgb(228, 232, 238);
        g.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(243, 246, 251);
        g.DefaultCellStyle.SelectionBackColor = Color.FromArgb(187, 222, 251);
        g.DefaultCellStyle.SelectionForeColor = Ink;
        g.AllowUserToResizeRows = false;
        g.RowTemplate.Height = h;
        foreach (DataGridViewRow r in g.Rows) r.Height = h;
    }
}
