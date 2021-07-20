using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Group
{
    [StructLayout(LayoutKind.Explicit, Size = 0x28)]
    public struct GroupDataEntry : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint FileNameIdx;

        [FieldOffset(0xC)]
        public uint ExternalFileRef;

        [FieldOffset(0x10)]
        public uint MetadataFileOffset;

        [FieldOffset(0x14)]
        public uint MetadataFileSize;

        [FieldOffset(0x18)]
        public uint AudioDataFileOffset;

        [FieldOffset(0x1C)]
        public uint AudioDataFileSize;

        [FieldOffset(0x24)]
        public uint SubsectionOffset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref FileNameIdx);
            Utils.ReverseEndianness(ref ExternalFileRef);
            Utils.ReverseEndianness(ref MetadataFileOffset);
            Utils.ReverseEndianness(ref MetadataFileSize);
            Utils.ReverseEndianness(ref AudioDataFileOffset);
            Utils.ReverseEndianness(ref AudioDataFileSize);
            Utils.ReverseEndianness(ref SubsectionOffset);
        }
    }
}