public struct StringPosition
{
    public string str;
    public int startOffset;
    public int endOffset;
    public int Length() => endOffset - startOffset + 1;
    public bool Empty() => startOffset < 0 || endOffset < 0;
}

public static class ExtensionMethods
{
    public static StringPosition Between(this string source, char start, char end)
    {
        int startIndex = source.IndexOf(start);
        if (startIndex == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        int endIndex = source.IndexOf(end, startIndex + 1);
        if (endIndex == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        return new StringPosition { str = source.Substring(startIndex + 1, endIndex - startIndex - 1), startOffset = startIndex, endOffset = endIndex };
    }

    public static StringPosition Between(this string source, string start, string end, int startIndex = 0)
    {
        int startPos = source.IndexOf(start, startIndex);
        if (startPos == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        startPos += start.Length;
        int endPos = source.IndexOf(end, startPos);
        if (endPos == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        return new StringPosition { str = source.Substring(startPos, endPos - startPos), startOffset = startPos, endOffset = endPos };
    }

    public static StringPosition Between(this string source, char start, string end)
    {
        int startIndex = source.IndexOf(start);
        if (startIndex == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        startIndex += 1;
        int endIndex = source.IndexOf(end, startIndex);
        if (endIndex == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        return new StringPosition { str = source.Substring(startIndex, endIndex - startIndex), startOffset = startIndex, endOffset = endIndex };
    }

    public static StringPosition Between(this string source, string start, char end)
    {
        int startPos = source.IndexOf(start);
        if (startPos == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        startPos += start.Length;
        int endPos = source.IndexOf(end, startPos);
        if (endPos == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        return new StringPosition { str = source.Substring(startPos, endPos - startPos), startOffset = startPos, endOffset = endPos };
    }

    public static StringPosition Find(this string source, string target)
    {
        int idx = source.IndexOf(target);
        if (idx == -1)
            return new StringPosition { str = string.Empty, startOffset = -1, endOffset = -1 };
        return new StringPosition { str = target, startOffset = idx, endOffset = idx + target.Length - 1 };
    }

    public static string Append(this string source, char c)
    {
        return source + c;
    }
}
