using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SlavApp.ImageFinder
{
    /// Homepage: http://www.idle-natura-system.com
    /// Author: WonYoung(Brad) Jeong
    /// 
    /// 64bit Integer is expressed in even any type beside bit since FieldOffse's unit is byte.
    /// So, you must create your own bit express.
    ///
    /// 1. Size
    /// This field must be equal or greater than the total size, in bytes, 
    /// of the members of the class or structure.
    ///
    /// 2. Pack 
    /// This field determines how many bytes will be used(or, n-byte unit).
    ///  ex) Pack n 
    ///     struct MyStruct
    ///     {
    ///        byte B0;
    ///        byte B1;
    ///        byte B2;
    ///        byte B3;
    ///     }
    ///
    ///   B0 will still begin at offset 0 (byte 0).
    ///   B1 will begin at offset n (byte n).
    ///   B2 will begin at offset n*2 (byte 2n).
    ///   B3 will begin at offset n*3 (byte 3n).
    ///
    [StructLayout(LayoutKind.Explicit, Size = 8, Pack = 1, CharSet = CharSet.Ansi)]
    public struct Any64
    {
        ///
        ///  express 64bits to 64bits integer.
        ///
        #region Int64
        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public Int64 INT64;
        #endregion //Int64

        ///
        ///  express 64bits to 64bits unsigned integer.
        ///
        #region UInt64
        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public UInt64 UINT64;
        #endregion //UInt64

        ///
        ///  express 64bits to double.
        ///
        #region double
        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public double DOUBLE;
        #endregion //double

        ///
        ///  express 64bits to 32bits float.
        ///
        #region float
        [FieldOffset(0)]        // 'FieldOffset' offsets by byte.
        public float FLOAT_0;
        [FieldOffset(4)]        // float or Single is 4 bytes. So, it is FieldOffset(4).
        public float FLOAT_1;
        #endregion //float

        ///
        /// express 64bits to 32bits unsigned integer.
        ///
        #region Uint32
        [FieldOffset(0)]
        public uint UINT32_0;
        [FieldOffset(4)]       //uint is 4 bytes. So, it is FieldOffset(4).
        public uint UINT32_1;
        #endregion //Uint32

        ///
        /// express 64bits to 16bits unsigned integer.
        ///
        #region UInt16
        [FieldOffset(0)]
        public ushort UINT16_0;
        [FieldOffset(2)]       //ushort is 2 bytes. So, it is FieldOffset(2).
        public ushort UINT16_1;
        [FieldOffset(4)]
        public ushort UINT16_2;
        [FieldOffset(6)]
        public ushort UINT16_3;
        #endregion //UInt16

        ///
        /// express 64bits to 8bits unsigned integer.
        ///
        #region UInt8
        [FieldOffset(0)]
        public byte UINT8_0;
        [FieldOffset(1)]       //byte is 1 byte. So, it is FieldOffset(1). increase one by one.
        public byte UINT8_1;
        [FieldOffset(2)]
        public byte UINT8_2;
        [FieldOffset(3)]
        public byte UINT8_3;
        [FieldOffset(4)]
        public byte UINT8_4;
        [FieldOffset(5)]
        public byte UINT8_5;
        [FieldOffset(6)]
        public byte UINT8_6;
        [FieldOffset(7)]
        public byte UINT8_7;
        #endregion //Int8

        ///
        /// express 64bits to 32bits integer.
        ///
        #region int32
        [FieldOffset(0)]
        public int INT32_0;
        [FieldOffset(4)]       //int is 4 bytes. So, it is FieldOffset(4).
        public int INT32_1;
        #endregion //int32

        ///
        /// express 64bits to 16bits integer.
        ///
        #region Int16
        [FieldOffset(0)]
        public short INT16_0;
        [FieldOffset(2)]      //short is 2 bytes. So, it is FieldOffset(2).
        public short INT16_1;
        [FieldOffset(4)]
        public short INT16_2;
        [FieldOffset(6)]
        public short INT16_3;
        #endregion //Int16

        ///
        /// express 64bits to 8bits integer.
        ///
        #region Int8
        [FieldOffset(0)]
        public sbyte INT8_0;
        [FieldOffset(1)]      //sbyte is 1 byte. So, it is FieldOffset(1). increase one by one.
        public sbyte INT8_1;
        [FieldOffset(2)]
        public sbyte INT8_2;
        [FieldOffset(3)]
        public sbyte INT8_3;
        [FieldOffset(4)]
        public sbyte INT8_4;
        [FieldOffset(5)]
        public sbyte INT8_5;
        [FieldOffset(6)]
        public sbyte INT8_6;
        [FieldOffset(7)]
        public sbyte INT8_7;
        #endregion //Int8

        ///
        /// express 64bits to high bits and low bits.
        /// It was made with 'private' for accessing to the index with 'this'.
        ///
        #region Bit            /// 'FieldOffset' is byte unit. So, we need  BitVector unavoidably.
        [FieldOffset(0)]
        private BitVector32 LowBits;
        [FieldOffset(4)]       //BitVector32 is 4 bytes, So, it is FieldOffset(4).
        private BitVector32 HighBits;

        ///
        /// express 64bits to bits.
        /// It was made with 'private' for accessing to the index with 'this'.
        ///
        [FieldOffset(0)]
        private BitVector64 Bits;
        #endregion //Bit

        // the index of BitVector64 is not convenient.
        // this make access to index convenient.
        public bool this[int index]
        {
            get { return Bits[(long)(1 << index)]; }
            set { Bits[(long)(1 << index)] = value; }
        }
    }
}
