namespace ScriptureLookup;

public class CachedPassage
{
    public string PassageKey { get; set; } = "";
    public string Canonical { get; set; } = "";
    public string Text { get; set; } = "";
    public string CachedAtUtc { get; set; } = "";
}