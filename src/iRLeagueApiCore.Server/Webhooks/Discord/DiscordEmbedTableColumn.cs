namespace iRLeagueApiCore.Server.Webhooks.Discord;

internal sealed class DiscordEmbedTableColumn
{
    public int Width { get; set; }
    public string Header { get; set; }
    public string? Abbreviation { get; set; }
    public List<string> Values { get; set; }
    public bool AlignRight { get; set; }
    public bool Expand { get; set; }

    public DiscordEmbedTableColumn(int width, string header, List<string> values, bool alignRight, bool expand, string? abbreviation)
    {
        Width = width;
        Header = header;
        Abbreviation = abbreviation;
        Values = values;
        AlignRight = alignRight;
        Expand = expand;
    }
}
