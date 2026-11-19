namespace Sur.Api;

static partial class Fn
{
    public static List<string> SnapshotLines(IReadOnlyList<SnapshotPart> parts)
    {
        var lines = new List<string>();
        foreach (var part in parts)
        {
            switch (part.Kind)
            {
                case SnapshotPartKind.Title:
                case SnapshotPartKind.Subtitle:
                case SnapshotPartKind.Section:
                    lines.Add(part.Value);
                    break;
                case SnapshotPartKind.Field:
                    lines.Add(Csv(part.Label, part.Value));
                    break;
                case SnapshotPartKind.Note:
                    lines.Add(part.Value);
                    break;
                case SnapshotPartKind.Blank:
                    lines.Add("");
                    break;
            }
        }
        return lines;
    }

    static string Csv(string key, string? value)
    {
        var text = value ?? "";
        return key + ";" + text.Replace(";", ",").Replace("\r", " ").Replace("\n", " ");
    }
}
