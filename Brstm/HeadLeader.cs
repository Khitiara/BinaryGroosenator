using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    [StructLayout(LayoutKind.Explicit)]
    public struct HeadLeader : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint Magic;

        [FieldOffset(0x4)]
        public uint SectionLen;

        [FieldOffset(0xC)]
        public uint Chunk1Offset;

        [FieldOffset(0x14)]
        public uint TrackDescTableOffset;

        [FieldOffset(0x1C)]
        public uint ChannelInfoOffset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref SectionLen);
            Utils.ReverseEndianness(ref Chunk1Offset);
            Utils.ReverseEndianness(ref TrackDescTableOffset);
            Utils.ReverseEndianness(ref ChannelInfoOffset);
        }
    }
}