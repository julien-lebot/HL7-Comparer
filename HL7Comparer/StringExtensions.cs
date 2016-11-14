using System;

namespace HL7Comparer
{
    public static class StringExtensions
    {
        public static int IndexOfNth(this string str, Func<char, bool> predicate, int numOccurence, int startIndex = 0)
        {
            int remaining = numOccurence;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (predicate(str[i]))
                {
                    remaining--;
                    if (remaining == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public static int IndexOfLine(this string str, int numOccurence, int startIndex = 0)
        {
            int remaining = numOccurence;
            for (int i = startIndex; i < str.Length; i++)
            {
                if (str[i] == '\r' || str[i] == '\n')
                {
                    if (i < str.Length && str[i + 1] == '\r' || str[i + 1] == '\n')
                    {
                        ++i;
                    }
                    remaining--;
                    if (remaining == 0)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }
    }
}