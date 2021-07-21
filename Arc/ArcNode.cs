using System;
using System.Runtime.InteropServices;
using NxCore;

namespace Arc
{
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
            Utils.ReverseEndianness(ref NameOffsetLower);
            Utils.ReverseEndianness(ref DataOffset);
            Utils.ReverseEndianness(ref Size);
        }
    }
}