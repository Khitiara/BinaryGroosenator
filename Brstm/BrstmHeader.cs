using System;
using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    [StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x40)]
    public struct BrstmHeader : IEndianAwareUnmanagedType
    {
        public uint   Magic;
        public ushort Bom;
        public byte   MajorVersion;
        public byte   MinorVersion;
        public uint   FileSize;
        public ushort HeaderSize;
        public ushort ChunkCount;
        public uint   HeadOffset;
        public uint   HeadChunkSize;
        public uint   AdpcOffset;
        public uint   AdpcSize;
        public uint   DataOffset;
        public uint   DataSize;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref Bom);
            if (Bom != 0xFEFF)
                throw new InvalidOperationException("Bad BOM");
            Utils.ReverseEndianness(ref FileSize);
            Utils.ReverseEndianness(ref HeaderSize);
            Utils.ReverseEndianness(ref ChunkCount);
            Utils.ReverseEndianness(ref HeadOffset);
            Utils.ReverseEndianness(ref HeadChunkSize);
            Utils.ReverseEndianness(ref AdpcOffset);
            Utils.ReverseEndianness(ref AdpcSize);
            Utils.ReverseEndianness(ref DataOffset);
            Utils.ReverseEndianness(ref DataSize);
        }
    }
}