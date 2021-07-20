using System.Runtime.InteropServices;
using NxCore;

namespace Brsar
{
    [StructLayout(LayoutKind.Explicit)]
    public struct InfoHeader : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint Magic;

        [FieldOffset(0x4)]
        public uint SectionSize;

        [FieldOffset(0xC)]
        public uint SoundDataOffset;

        [FieldOffset(0x14)]
        public uint SoundbankOffset;

        [FieldOffset(0x1C)]
        public uint PlayerInfoOffset;

        [FieldOffset(0x24)]
        public uint CollectionTableOffset;

        [FieldOffset(0x2C)]
        public uint GroupTableOffset;

        [FieldOffset(0x34)]
        public uint SoundCountTableOffset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref SectionSize);
            Utils.ReverseEndianness(ref SoundDataOffset);
            Utils.ReverseEndianness(ref SoundbankOffset);
            Utils.ReverseEndianness(ref PlayerInfoOffset);
            Utils.ReverseEndianness(ref CollectionTableOffset);
            Utils.ReverseEndianness(ref GroupTableOffset);
            Utils.ReverseEndianness(ref SoundCountTableOffset);
        }
    }
}