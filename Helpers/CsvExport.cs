using System.Globalization;
using System.Text;
namespace MarsaKasir.Helpers;

public static class CsvExport
{
    // Pemisah ";" dan UTF-8 BOM supaya langsung rapi di Excel berbahasa Indonesia
    public static void Save(DataGridView grid, string path)
    {
        var cols = grid.Columns.Cast<DataGridViewColumn>().Where(c => c.Visible).ToList();
        var sb = new StringBuilder();
        sb.AppendLine(string.Join(";", cols.Select(c => Esc(c.HeaderText))));
        foreach (DataGridViewRow r in grid.Rows)
            sb.AppendLine(string.Join(";", cols.Select(c =>
                Esc(Convert.ToString(r.Cells[c.Index].Value, CultureInfo.InvariantCulture) ?? ""))));
        File.WriteAllText(path, sb.ToString(), new UTF8Encoding(true));
    }

    static string Esc(string s) =>
        s.IndexOfAny(new[] { ';', '"', '\n', '\r' }) >= 0 ? "\"" + s.Replace("\"", "\"\"") + "\"" : s;
}
