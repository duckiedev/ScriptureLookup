namespace ScriptureLookup;

public static class VerseOfTheDay
{
    private static readonly string[] Verses =
    {
        "John 3:16",
        "Psalm 23:1",
        "Philippians 4:13",
        "Romans 8:28",
        "Proverbs 3:5-6",
        "Isaiah 41:10",
        "Matthew 6:33",
        "Jeremiah 29:11",
        "1 Corinthians 13:4-7",
        "Psalm 46:1"
    };

    public static string GetTodaysVerse()
    {
        var dayOfYear = DateTime.UtcNow.DayOfYear;
        var index = dayOfYear % Verses.Length;
        return Verses[index];
    }
}