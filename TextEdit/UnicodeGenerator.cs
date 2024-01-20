namespace TextEdit;

public static class UnicodeGenerator
{
    private static readonly Random Random = new Random();

    public static char GetRandomUnicodeCharacter()
    {
        var includeRanges = new[]
        {
            (0x0021, 0x0021),
            (0x0023, 0x0026),
            (0x0028, 0x007E),
            (0x00A1, 0x00AC),
            (0x00AE, 0x00FF),
            (0x0100, 0x017F),
            (0x0180, 0x024F),
            (0x2C60, 0x2C7F),
            (0x16A0, 0x16F0),
            (0x0370, 0x0377),
            (0x037A, 0x037E),
            (0x0384, 0x038A),
            (0x038C, 0x038C),
        };

        int rangeIndex = Random.Next(includeRanges.Length);
        var range = includeRanges[rangeIndex];
        int codePoint = Random.Next(range.Item1, range.Item2 + 1);
        return (char)codePoint;
    }
}
