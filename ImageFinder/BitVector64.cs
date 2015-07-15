﻿
//
// System.Collections.Specialized.BitVector32.cs
//
// Author:
//   Miguel de Icaza
//   Lawrence Pit
//   Andrew Birkett
//   Andreas Nahr
//
//   WonYoung(Brad) Jeong : converted 32bits to 64bits
//
//
// (C) Ximian, Inc.  <a href="http://www.ximian.com">http://www.ximian.com
// Copyright (C) 2005 Novell, Inc (<a href="http://www.novell.com">http://www.novell.com</a>)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//

using System;
using System.Collections.Specialized;
using System.Text;

namespace SlavApp.ImageFinder
{
    public struct BitVector64
    {
        private long data;

        #region Section

        ///
        /// Section
        ///
        public struct Section
        {
            private short mask;
            private short offset;

            internal Section(short mask, short offset)
            {
                this.mask = mask;
                this.offset = offset;
            }

            public short Mask
            {
                get { return mask; }
            }

            public short Offset
            {
                get { return offset; }
            }

#if NET_2_0
        public static bool operator == (Section v1, Section v2)
        {
            return v1.mask == v2.mask &&
                    v1.offset == v2.offset;
        }

        public static bool operator != (Section v1, Section v2)
        {
            return v1.mask != v2.mask &&
                    v1.offset != v2.offset;
        }

        public bool Equals (Section obj)
        {
            return this.mask == obj.mask &&
                    this.offset == obj.offset;
        }
#endif

            public override bool Equals(object o)
            {
                if (!(o is Section))
                    return false;

                Section section = (Section)o;
                return this.mask == section.mask &&
                        this.offset == section.offset;
            }

            public override int GetHashCode()
            {
                return (((Int16)mask).GetHashCode() << 16) +
                        ((Int16)offset).GetHashCode();
            }

            public override string ToString()
            {
                return "Section{0x" + Convert.ToString(mask, 16) +
                        ", 0x" + Convert.ToString(offset, 16) + "}";
            }

            public static string ToString(Section value)
            {
                StringBuilder b = new StringBuilder();
                b.Append("Section{0x");
                b.Append(Convert.ToString(value.Mask, 16));
                b.Append(", 0x");
                b.Append(Convert.ToString(value.Offset, 16));
                b.Append("}");

                return b.ToString();
            }
        }

        #endregion Section

        #region Constructors

        public BitVector64(BitVector64 source)
        {
            this.data = source.data;
        }

        public BitVector64(BitVector32 source)
        {
            this.data = source.Data;
        }

        public BitVector64(long source)
        {
            this.data = source;
        }

        public BitVector64(int init)
        {
            this.data = init;
        }

        #endregion Constructors

        #region Properties

        public long Data
        {
            get { return this.data; }
        }

        public long this[BitVector64.Section section]
        {
            get
            {
                return ((data >> section.Offset) & section.Mask);
            }

            set
            {
                if (value < 0)
                    throw new ArgumentException("Section can't hold negative values");
                if (value > section.Mask)
                    throw new ArgumentException("Value too large to fit in section");
                this.data &= ~(section.Mask << section.Offset);
                this.data |= (value << section.Offset);
            }
        }

        public bool this[long mask]
        {
            get
            {
#if NET_2_0
            return (this.data & mask) == mask;
#else
                long tmp = /*(uint)*/this.data;
                return (tmp & (long)mask) == (long)mask;
#endif
            }

            set
            {
                if (value)
                    this.data |= mask;
                else
                    this.data &= ~mask;
            }
        }

        #endregion Properties

        // Methods
        public static long CreateMask()
        {
            return CreateMask(0);   // 1;
        }

        public static long CreateMask(long prev)
        {
            if (prev == 0)
                return 1;
            if (prev == Int64.MinValue)
                throw new InvalidOperationException("all bits set");
            return prev << 1;
        }

        public static Section CreateSection(int maxValue)
        {
            return CreateSection(maxValue, new Section(0, 0));
        }

        public static Section CreateSection(int maxValue, BitVector64.Section previous)
        {
            if (maxValue < 1)
                throw new ArgumentException("maxValue");

            int bit = HighestSetBit(maxValue) + 1;
            int mask = (1 << bit) - 1;
            int offset = previous.Offset + NumberOfSetBits(previous.Mask);

            if (offset > 64)
            {
                throw new ArgumentException("Sections cannot exceed 64 bits in total");
            }

            return new Section((short)mask, (short)offset);
        }

        public override bool Equals(object o)
        {
            if (!(o is BitVector64))
                return false;

            return data == ((BitVector64)o).data;
        }

        public override int GetHashCode()
        {
            return data.GetHashCode();
        }

        public override string ToString()
        {
            return ToString(this);
        }

        public static string ToString(BitVector64 value)
        {
            StringBuilder sb = new StringBuilder(0x2d);
            sb.Append("BitVector64{");
            ulong data = (ulong)value.Data;
            for (int i = 0; i < 0x40; i++)
            {
                sb.Append(((data & 0x8000000000000000) == 0) ? '0' : '1');
                data = data << 1;
            }

            sb.Append("}");
            return sb.ToString();

            //StringBuilder b = new StringBuilder();
            //b.Append("BitVector64{");
            //ulong mask = (ulong)Convert.ToInt64(0x8000000000000000);
            //while (mask > 0)
            //{
            //    b.Append((((ulong)value.Data & mask) == 0) ? '0' : '1');
            //    mask >>= 1;
            //}
            //b.Append('}');
            //return b.ToString();
        }

        // Private utilities
        private static int NumberOfSetBits(int i)
        {
            int count = 0;
            for (int bit = 0; bit < 64; bit++)
            {
                int mask = 1 << bit;
                if ((i & mask) != 0)
                    count++;
            }
            return count;
        }

        private static int HighestSetBit(int i)
        {
            for (int bit = 63; bit >= 0; bit--)
            {
                int mask = 1 << bit;
                if ((mask & i) != 0)
                {
                    return bit;
                }
            }
            return -1;
        }
    }
}