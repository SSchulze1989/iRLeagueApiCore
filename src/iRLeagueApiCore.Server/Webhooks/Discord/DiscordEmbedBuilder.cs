using System.Text;

namespace iRLeagueApiCore.Server.Webhooks.Discord;

internal sealed class DiscordEmbedBuilder
{
    public string? Title { get; private set; }
    public string? Description { get; private set; }
    public DateTimeOffset? Timestamp { get; private set; }
    public int? Color { get; private set; }
    public Dictionary<string, object>? Footer { get; private set; }
    public List<Dictionary<string, object>> Fields { get; } = [];

    public DiscordEmbedBuilder SetTitle(string title)
    {
        if (title.Length > DiscordEmbedLimits.TitleMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(title), $"Title length exceeds maximum of {DiscordEmbedLimits.TitleMaxLength} characters.");
        }
        Title = title;
        return this;
    }

    public DiscordEmbedBuilder SetDescription(string description)
    {
        if (description.Length > DiscordEmbedLimits.DescriptionMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(description), $"Description length exceeds maximum of {DiscordEmbedLimits.DescriptionMaxLength} characters.");
        }
        Description = description;
        return this;
    }

    public DiscordEmbedBuilder SetTimestamp(DateTimeOffset timestamp)
    {
        Timestamp = timestamp;
        return this;
    }

    public DiscordEmbedBuilder SetColor(int color)
    {
        Color = color;
        return this;
    }

    public DiscordEmbedBuilder SetFooter(string text, string? iconUrl = null)
    {
        if (text.Length > DiscordEmbedLimits.FooterTextMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(text), $"Footer text length exceeds maximum of {DiscordEmbedLimits.FooterTextMaxLength} characters.");
        }
        Footer = new Dictionary<string, object> { { "text", text } };
        if (!string.IsNullOrEmpty(iconUrl))
        {
            Footer["icon_url"] = iconUrl;
        }
        return this;
    }

    public DiscordEmbedBuilder AddField(string name, string value, bool inline = false)
    {
        if (Fields.Count >= DiscordEmbedLimits.MaxFields)
        {
            throw new InvalidOperationException($"Cannot add more than {DiscordEmbedLimits.MaxFields} fields to an embed.");
        }
        if (name.Length > DiscordEmbedLimits.FieldNameMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(name), $"Field name length exceeds maximum of {DiscordEmbedLimits.FieldNameMaxLength} characters.");
        }
        if (value.Length > DiscordEmbedLimits.FieldValueMaxLength)
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Field value length exceeds maximum of {DiscordEmbedLimits.FieldValueMaxLength} characters.");
        }
        Fields.Add(new Dictionary<string, object>
        {
            { "name", name },
            { "value", value },
            { "inline", inline }
        });
        return this;
    }

    public DiscordEmbedBuilder AddTableFieldsWithSplitting(string name, DiscordEmbedTable table)
    {
        // count number of fields needed for the table
        int numRows = table.Columns.MaxBy(c => c.Values.Count)?.Values.Count ?? 0;
        // account for code block characters and new lines
        int maxTableChars = DiscordEmbedLimits.FieldValueMaxLength - ("``````".Length + 2 * Environment.NewLine.Length);

        int currentRow = 0;
        bool firstField = true;
        while (currentRow < numRows)
        {
            var sb = new StringBuilder();
            if (currentRow == 0)
            {
                table.AppendHeader(sb);
            }
            while (currentRow < numRows)
            {
                // check if adding another row would exceed the field value limit
                if (sb.Length + table.CurrentTableWidth + Environment.NewLine.Length > maxTableChars)
                {
                    break;
                }
                // append the current row
                table.AppendRow(sb, currentRow++);
            }
            var fieldName = firstField ? name : "";
            AddField(fieldName, $"```\n{sb.ToString().TrimEnd()}\n```", false);
            firstField = false;
            if (Fields.Count >= DiscordEmbedLimits.MaxFields)
            {
                break;
            }
        }
        return this;
    }

    public Dictionary<string, object> Build()
    {
        var embed = new Dictionary<string, object>();
        if (Title != null) embed["title"] = Title;
        if (Description != null) embed["description"] = Description;
        if (Timestamp != null) embed["timestamp"] = Timestamp.Value.UtcDateTime;
        if (Color != null) embed["color"] = Color.Value;
        if (Footer != null) embed["footer"] = Footer;
        if (Fields.Count > 0) embed["fields"] = Fields;
        return embed;
    }
}
