using System;
using System.Runtime.InteropServices;
using NxCore;

namespace Arc
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct ArcHeader : IEndianAwareUnmanagedType
    {
        public uint Magic;
        public uint RootNodeOffset;
        public uint HeaderSize;
        public uint DataOffset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref RootNodeOffset);
            Utils.ReverseEndianness(ref HeaderSize);
            Utils.ReverseEndianness(ref DataOffset);
        }
    }

    public enum ArcNodeType : byte
    {
        File      = 0,
        Directory = 1
    }

    [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    public struct ArcNode : IEndianAwareUnmanagedType
    {
        public  ArcNodeType Type;
        private byte        NameOffsetUpper;
        private ushort      NameOffsetLower;
        public  uint        DataOffset;
        public  uint        Size;

        public uint NameOffset => (uint)((NameOffsetUpper << 16) + NameOffsetLower);

        public void FixEndian() {
            Span<byte> shit = MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref this, 1));
            shit.Slice(1, 3).Reverse();
            shit.Slice(4, 4).Reverse();
            shit.Slice(8, 4).Reverse();
        }
    }
}