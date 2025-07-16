namespace ExtensionMethods
{
    public struct StringPosition
    {
        public int startOffset;
        public int length;
        public string str;
        public bool Empty() => startOffset == -1;
        public int Length() => length;
    }

    public static class StringExtensions
    {
        public static StringPosition Between(this string input, char start, char end, int startIndex = 0)
        {
            int startPos = input.IndexOf(start, startIndex);
            if (startPos == -1) return new StringPosition { startOffset = -1, str = string.Empty, length = 0 };
            int endPos = input.IndexOf(end, startPos + 1);
            if (endPos == -1) return new StringPosition { startOffset = -1, str = string.Empty, length = 0 };
            string inner = input.Substring(startPos + 1, endPos - startPos - 1);
            return new StringPosition { startOffset = startPos, length = endPos - startPos + 1, str = inner };
        }

        public static StringPosition Between(this string input, string start, string end, int startIndex = 0)
        {
            int startPos = input.IndexOf(start, startIndex, System.StringComparison.Ordinal);
            if (startPos == -1) return new StringPosition{startOffset=-1,str="",length=0};
            startPos += start.Length;
            int endPos = input.IndexOf(end, startPos, System.StringComparison.Ordinal);
            if (endPos == -1) return new StringPosition{startOffset=-1,str="",length=0};
            string inner = input.Substring(startPos, endPos - startPos);
            return new StringPosition{startOffset=startPos,length=endPos - startPos + end.Length,str=inner};
        }

        public static StringPosition Find(this string input, string target)
        {
            int idx = input.IndexOf(target, System.StringComparison.Ordinal);
            if (idx == -1) return new StringPosition{startOffset=-1,str="",length=0};
            return new StringPosition{startOffset=idx,length=target.Length,str=target};
        }

        public static string Append(this string input, char c) => input + c;

        public static char Last(this string input) => input[^1];

        public static long ToPointer(this string input) => 0; // dummy
    }
}
