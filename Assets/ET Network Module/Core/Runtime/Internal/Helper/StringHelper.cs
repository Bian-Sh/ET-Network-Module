using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ET
{
    public static class StringHelper
    {
        public static IEnumerable<byte> ToBytes(this string str) => Encoding.Default.GetBytes(str);
        public static byte[] ToByteArray(this string str) => Encoding.Default.GetBytes(str);
        public static byte[] ToUtf8(this string str) => Encoding.UTF8.GetBytes(str);
        public static byte[] HexToBytes(this string hexString)
        {
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            var hexAsBytes = new byte[hexString.Length / 2];
            for (int index = 0; index < hexAsBytes.Length; index++)
            {
                string byteValue = "";
                byteValue += hexString[index * 2];
                byteValue += hexString[index * 2 + 1];
                hexAsBytes[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return hexAsBytes;
        }
        public static string ToStr<T>(this List<T> list)
        {
            StringBuilder sb = new StringBuilder();
            foreach (T t in list)
            {
                sb.Append(t);
                sb.Append(",");
            }
            return sb.ToString();
        }
        public static string ToStr<T>(this T[] args, int index=0, int count=-1)
        {
            if (args == null)
            {
                return "";
            }
            if (count==-1)
            {
                count = args.Length;
            }
            string argStr = " [";
            for (int arrIndex = index; arrIndex < count + index; arrIndex++)
            {
                argStr += args[arrIndex];
                if (arrIndex != args.Length - 1)
                {
                    argStr += ", ";
                }
            }

            argStr += "]";
            return argStr;
        }
    }
}