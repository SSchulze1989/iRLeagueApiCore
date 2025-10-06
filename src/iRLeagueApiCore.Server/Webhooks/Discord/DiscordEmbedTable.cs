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

    public DiscordEmbedTable AddColumn(int width, string header, List<string> values)
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
        Columns.Add(new DiscordEmbedTableColumn(width, header, values));
        return this;
    }

    public DiscordEmbedTable AddColumn<T>(int width, string header, IEnumerable<T> values, Func<T, string> func)
    {
        var stringValues = values.Select(func).ToList();
        return AddColumn(width, header, stringValues);
    }

    public StringBuilder AppendHeader(StringBuilder sb)
    {
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            if (i > 0)
            {
                sb.Append(delimiter);
            }
            sb.Append(column.Header.PadRight(column.Width).AsSpan(0, column.Width));
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
        for (int i = 0; i < Columns.Count; i++)
        {
            var column = Columns[i];
            if (i > 0)
            {
                sb.Append(delimiter);
            }
            string cellValue = rowIndex < column.Values.Count ? column.Values[rowIndex] : "";
            sb.Append(cellValue.PadRight(column.Width).AsSpan(0, column.Width));
        }
        sb.AppendLine();
        return sb;
    }
}
