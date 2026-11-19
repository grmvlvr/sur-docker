namespace Sur.Api;

enum SnapshotPartKind
{
    Title,
    Subtitle,
    Section,
    Field,
    Note,
    Blank
}

sealed record SnapshotPart(SnapshotPartKind Kind, string Label = "", string Value = "");
