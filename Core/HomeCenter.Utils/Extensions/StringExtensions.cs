using System;

namespace HomeCenter.Utils.Extensions
{
    public static class StringExtensions
    {
        public static int Compare(this string orginalText, string comparedText) => string.Compare(orginalText, comparedText, StringComparison.OrdinalIgnoreCase);

        public static byte[] ToBytes(this string byteString)
        {
            // Get the separator character.
            if (byteString.Length > 2)
            {
                char separator = byteString[2];

                // Split at the separators.
                string[] pairs = byteString.Split(separator);
                byte[] bytes = new byte[pairs.Length];
                for (int i = 0; i < pairs.Length; i++)
                {
                    bytes[i] = Convert.ToByte(pairs[i], 16);
                }
                return bytes;
            }
            else
            {
                return new byte[] { Convert.ToByte(byteString, 16) };
            }
            
        }
    }
}