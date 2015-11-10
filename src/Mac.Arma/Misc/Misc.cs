// <copyright company="SIX Networks GmbH" file="Misc.cs">
//     Copyright (c) SIX Networks GmbH. All rights reserved. Do not remove this notice.
// </copyright>

using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Mac.Arma.Files
{
    static class BinaryFile
    {
        internal static string ReadString(Stream sr) {
            var ret = new StringBuilder();
            while (true) {
                var c = sr.ReadByte();
                if (c <= 0)
                    break;
                ret.Append((char) c);
            }
            return ret.ToString();
        }

        internal static UInt32 ReadUInt32(Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, 4);
            return BitConverter.ToUInt32(buf, 0);
        }

        internal static Int32 ReadInt32(Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, 4);
            return BitConverter.ToInt32(buf, 0);
        }

        internal static Int16 ReadInt16(Stream sr) {
            var buf = new byte[2];
            sr.Read(buf, 0, 2);
            return BitConverter.ToInt16(buf, 0);
        }

        internal static float ReadFloat32(Stream sr) {
            var buf = new byte[4];
            sr.Read(buf, 0, 4);
            return BitConverter.ToSingle(buf, 0);
        }

        internal static void WriteString(Stream sr, string n) {
            sr.Write(Encoding.UTF8.GetBytes(n), 0, n.Length);
            sr.WriteByte(0);
        }

        internal static void WriteByte(Stream sr, byte b) {
            sr.WriteByte(b);
        }

        internal static void WriteUInt32(Stream sr, UInt32 n) {
            var buf = BitConverter.GetBytes(n);
            sr.Write(buf, 0, 4);
        }

        internal static void WriteFloat32(Stream sr, float n) {
            sr.Write(BitConverter.GetBytes(n), 0, 4);
        }

        //this is a special kind of Arma number
        internal static int ReadCompressedInteger(Stream s) {
            var a = 0;
            for (var bytePos = 0; bytePos < 5; bytePos++) {
                var c = s.ReadByte();
                a |= (c & 0x7f) << (bytePos*7);
                if ((c & 0x80) == 0)
                    break;
            }
            return a;
        }

        //this is a special kind of Arma number
        internal static void WriteCompressedInteger(Stream sr, int n) {
            var ret = new MemoryStream();
            do {
                var o = (byte) (n & 0x7f);
                if (n > 0x7f)
                    o |= 0x80;
                ret.WriteByte(o);
                n = n >> 7;
            } while (n > 0);
            var reta = ret.ToArray();
            sr.Write(reta, 0, reta.Length);
        }
    }

    /// <summary>
    ///     Helper for common text-based functions peculiar to Arma
    /// </summary>
    public static class TextFile
    {
        /// <summary>
        ///     Quotes a string For inclusion in an Arma text file
        /// </summary>
        /// <remarks>
        ///     Converts a string such as [this includes a "string"] into
        ///     ["this includes a ""string"""].
        /// </remarks>
        /// <param name="value">The string to quote</param>
        /// <returns>The quoted string</returns>
        public static string Quote(string value) {
            //Expand quotes and quote whole string
            value = value.Replace("\"", "\"\"");
            value = "\"" + value + "\"";
            return value;
        }

        /// <summary>
        ///     Unquotes a string from an Arma text file
        /// </summary>
        /// <remarks>
        ///     Converts a string such as ["this includes a ""string"""] into
        ///     [this includes a "string"].
        /// </remarks>
        /// <param name="value">The string to dequote</param>
        /// <returns>The dequoted string</returns>
        public static string Unquote(string value) {
            //remove start and end quotes and 
            //replace double-quotes by single
            if (value.StartsWith("\"", StringComparison.Ordinal))
                value = value.Remove(0, 1);
            if (value.EndsWith("\"", StringComparison.Ordinal))
                value = value.Remove(value.Length - 1, 1);
            value = value.Replace("\"\"", "\"");
            return value;
        }


        /// <summary>
        ///     Provides the correct arma formatting for any kind of basic object placed into a text file
        /// </summary>
        /// <param name="value">The object to place in the text file</param>
        /// <returns>The string that should be used to represent the object</returns>
        public static string ToString(Object value) {
            var s = value as string;
            if (s != null)
                return Quote(s);
            if (value is Int32)
                return ((Int32) value).ToString(CultureInfo.InvariantCulture);
            if (value is float)
                return ((float) value).ToString(CultureInfo.InvariantCulture);
            return value.ToString();
        }

        /// <summary>
        ///     Attempts to convert a string from an Arma text file into the appropriate kind of
        ///     object
        /// </summary>
        /// <param name="value">The string representing the object</param>
        /// <returns>An object of the appropriate type or else null if the string didn't fit any of the known types</returns>
        /// <example>
        ///     <code>
        /// var o = TryParse("\"a string\""); //o is a String with value "a string" 
        /// var o = TryParse("150"); //o is an Int32 with value 150 
        /// var o = TryParse("1.23"); //o is a float with value 1.23 
        /// </code>
        /// </example>
        public static Object TryParse(string value) {
            value = value.Trim();
            if (value.StartsWith("\"", StringComparison.Ordinal))
                return Unquote(value);
            //important - we need to try the int first because a float can match an integer format
            Int32 i;
            if (Int32.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out i))
                return i;
            float f;
            if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out f))
                return f;
            return null;
        }
    }
}