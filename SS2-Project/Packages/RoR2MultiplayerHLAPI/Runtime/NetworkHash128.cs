using System;
using System.Runtime.InteropServices;

namespace UnityEngine.Networking
{
    /// <summary>
    /// A 128 bit number used to represent assets in a networking context.
    /// </summary>
    // unrolled for your pleasure.
    [Serializable]
    [StructLayout(LayoutKind.Explicit)]
    public struct NetworkHash128 : IEquatable<NetworkHash128>
    {
        // struct cannot have embedded arrays..
        [FieldOffset(0)]
        public byte i0;
        [FieldOffset(1)]
        public byte i1;
        [FieldOffset(2)]
        public byte i2;
        [FieldOffset(3)]
        public byte i3;
        [FieldOffset(4)]
        public byte i4;
        [FieldOffset(5)]
        public byte i5;
        [FieldOffset(6)]
        public byte i6;
        [FieldOffset(7)]
        public byte i7;
        [FieldOffset(8)]
        public byte i8;
        [FieldOffset(9)]
        public byte i9;
        [FieldOffset(10)]
        public byte i10;
        [FieldOffset(11)]
        public byte i11;
        [FieldOffset(12)]
        public byte i12;
        [FieldOffset(13)]
        public byte i13;
        [FieldOffset(14)]
        public byte i14;
        [FieldOffset(15)]
        public byte i15;

        [FieldOffset(0)]
        private ulong i0_7;

        [FieldOffset(8)]
        private ulong i8_15;

        /// <summary>
        /// Resets the value of a NetworkHash to zero (invalid).
        /// </summary>
        public void Reset()
        {
            i0_7 = 0ul;
            i8_15 = 0ul;
        }

        /// <summary>
        /// A valid NetworkHash has a non-zero value.
        /// </summary>
        /// <returns>True if the value is non-zero.</returns>
        public bool IsValid()
        {
            return (i0_7 | i8_15) != 0;
        }

        static int HexToNumber(char c)
        {
            if (c >= '0' && c <= '9')
                return c - '0';
            if (c >= 'a' && c <= 'f')
                return c - 'a' + 10;
            if (c >= 'A' && c <= 'F')
                return c - 'A' + 10;
            return 0;
        }

        /// <summary>
        /// This parses the string representation of a NetworkHash into a binary object.
        /// <code>
        /// using UnityEngine;
        /// using UnityEngine.Networking;
        ///
        /// class HashTest : MonoBehaviour
        /// {
        ///    const string assetString = "0176acd452adc180";
        ///    NetworkHash128 assetId = NetworkHash128.Parse(assetString);
        ///
        ///    void Start()
        ///    {
        ///        Debug.Log("asset:" + assetId);
        ///    }
        /// }
        /// </code>
        /// </summary>
        /// <param name="text">A hex string to parse.</param>
        /// <returns>A 128 bit network hash object.</returns>
        public static NetworkHash128 Parse(string text)
        {
            NetworkHash128 hash = default(NetworkHash128);

            // add leading zeros if required
            int l = text.Length;
            if (l < 32)
            {
                string tmp = "";
                for (int i = 0; i < 32 - l; i++)
                {
                    tmp += "0";
                }
                text = (tmp + text);
            }

            hash.i0 = (byte)(HexToNumber(text[0]) * 16 + HexToNumber(text[1]));
            hash.i1 = (byte)(HexToNumber(text[2]) * 16 + HexToNumber(text[3]));
            hash.i2 = (byte)(HexToNumber(text[4]) * 16 + HexToNumber(text[5]));
            hash.i3 = (byte)(HexToNumber(text[6]) * 16 + HexToNumber(text[7]));
            hash.i4 = (byte)(HexToNumber(text[8]) * 16 + HexToNumber(text[9]));
            hash.i5 = (byte)(HexToNumber(text[10]) * 16 + HexToNumber(text[11]));
            hash.i6 = (byte)(HexToNumber(text[12]) * 16 + HexToNumber(text[13]));
            hash.i7 = (byte)(HexToNumber(text[14]) * 16 + HexToNumber(text[15]));
            hash.i8 = (byte)(HexToNumber(text[16]) * 16 + HexToNumber(text[17]));
            hash.i9 = (byte)(HexToNumber(text[18]) * 16 + HexToNumber(text[19]));
            hash.i10 = (byte)(HexToNumber(text[20]) * 16 + HexToNumber(text[21]));
            hash.i11 = (byte)(HexToNumber(text[22]) * 16 + HexToNumber(text[23]));
            hash.i12 = (byte)(HexToNumber(text[24]) * 16 + HexToNumber(text[25]));
            hash.i13 = (byte)(HexToNumber(text[26]) * 16 + HexToNumber(text[27]));
            hash.i14 = (byte)(HexToNumber(text[28]) * 16 + HexToNumber(text[29]));
            hash.i15 = (byte)(HexToNumber(text[30]) * 16 + HexToNumber(text[31]));

            return hash;
        }

        /// <summary>
        /// Returns a string representation of a NetworkHash object.
        /// </summary>
        /// <returns>A hex asset string.</returns>
        public override string ToString()
        {
            return String.Format("{0:x2}{1:x2}{2:x2}{3:x2}{4:x2}{5:x2}{6:x2}{7:x2}{8:x2}{9:x2}{10:x2}{11:x2}{12:x2}{13:x2}{14:x2}{15:x2}",
                i0, i1, i2, i3, i4, i5, i6, i7, i8, i9, i10, i11, i12, i13, i14, i15);
        }

        public bool Equals(NetworkHash128 other)
        {
            if (i0_7 == other.i0_7)
            {
                return i8_15 == other.i8_15;
            }
            return false;
        }

        public override bool Equals(object other)
        {
            if (!(other is NetworkHash128 other2))
            {
                return false;
            }
            return Equals(other2);
        }

        public override int GetHashCode()
        {
            return i0_7.GetHashCode();
        }
    }
}
