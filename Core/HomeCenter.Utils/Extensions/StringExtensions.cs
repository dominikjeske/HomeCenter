using System;

namespace HomeCenter.Utils.Extensions
{
    public static class StringExtensions
    {
        public static int Compare(this string orginalText, string comparedText) => string.Compare(orginalText, comparedText, StringComparison.OrdinalIgnoreCase);

        public static byte[] ToBytes(this string the_string)
        {
            // Get the separator character.
            char separator = the_string[2];

            // Split at the separators.
            string[] pairs = the_string.Split(separator);
            byte[] bytes = new byte[pairs.Length];
            for (int i = 0; i < pairs.Length; i++)
                bytes[i] = Convert.ToByte(pairs[i], 16);
            return bytes;
        }
    }
}