using System.Text;

namespace iRLeagueApiCore.Server.Webhooks.Discord;

internal sealed class DiscordEmbedTable
{
    private static readonly int maxTableWidth = 56;
    private readonly string delimiter;

    public int CurrentTableWidth => Columns.Sum(c => c.Width) + (Columns.Count - 1) * delimiter.Length;
    public int RemainingWidth => maxTableWidth - CurrentTableWidth;
    public int TableRows => Columns.MaxBy(c => c.Values.Count)?.Values.Count ?? 0;

    public List<DiscordEmbedTableColumn> Columns { get; } = [];

    public DiscordEmbedTable(string delimiter = " | ")
    {
        this.delimiter = delimiter;
    }

    public DiscordEmbedTable AddColumn(int width, string header, List<string> values, bool alignRight = false, bool expand = false, string? abbreviation = null)
    {
        if (values.Count == 0)
        {
            throw new ArgumentException("Values list cannot be empty", nameof(values));
        }
        if (width < 1)
        {
            // get auto column width
            width = values.MaxBy(x => x.Length)?.Length ?? 1;
        }
        if (width >= RemainingWidth)
        {
            width = RemainingWidth;
        }
        if (width <= 0)
        {
            throw new InvalidOperationException("Cannot add more columns, maximum table width reached");
        }
        Columns.Add(new DiscordEmbedTableColumn(width, header, values, alignRight, expand, abbreviation));
        return this;
    }

    public DiscordEmbedTable AddColumn<T>(int width, string header, IEnumerable<T> values, Func<T, string> func, bool alignRight = false, bool expand = false, string? abbreviation = null)
    {
        var stringValues = values.Select(func).ToList();
        return AddColumn(width, header, stringValues, alignRight: alignRight, expand: expand, abbreviation: abbreviation);
    }

    private void ExpandColumns()
    {
        var expandableColumns = Columns.Where(c => c.Expand).ToList();
        if (expandableColumns.Count == 0)
        {
            return;
        }
        int totalExpandableWidth = RemainingWidth;
        int extraWidthPerColumn = totalExpandableWidth / expandableColumns.Count;
        foreach (var column in expandableColumns)
        {
            column.Width += extraWidthPerColumn;
        }
        // If there's any remaining width due to integer division, add it to the first expandable column
        int remainingWidth = RemainingWidth;
        if (remainingWidth > 0)
        {
            expandableColumns[0].Width += remainingWidth;
        }
    }

    public StringBuilder AppendHeader(StringBuilder sb)
    {
        ExpandColumns();
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            if (i > 0)
            {
                sb.Append(delimiter);
            }
            var cellValue = (column.Header.Length > column.Width ? column.Abbreviation : column.Header) ?? column.Header;
            sb.Append(cellValue.PadRight(column.Width).AsSpan(0, column.Width));
        }
        sb.AppendLine();
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            if (i > 0)
            {
                sb.Append(delimiter);
            }
            sb.Append(new string('-', column.Width));
        }
        sb.AppendLine();
        return sb;
    }

    public StringBuilder AppendRow(StringBuilder sb, int rowIndex)
    {
        ExpandColumns();
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            if (i > 0)
            {
                sb.Append(delimiter);
            }
            var cellValue = rowIndex < column.Values.Count ? column.Values[rowIndex] : "";
            var alignedValue = column.AlignRight ? cellValue.PadLeft(column.Width) : cellValue.PadRight(column.Width);
            sb.Append(alignedValue.AsSpan(0, column.Width));
        }
        sb.AppendLine();
        return sb;
    }
}
