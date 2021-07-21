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
}