using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Group
{
    [StructLayout(LayoutKind.Sequential, Size = 0x18)]
    public struct GroupDataSubsection : IEndianAwareUnmanagedType
    {
        public uint GroupIdx;
        public uint MetadataRelOffset;
        public uint MetadataRelSize;
        public uint AudioDataRelOffset;
        public uint AudioDataRelSize;

        public void FixEndian() {
            Utils.ReverseEndianness(ref GroupIdx);
            Utils.ReverseEndianness(ref MetadataRelOffset);
            Utils.ReverseEndianness(ref MetadataRelSize);
            Utils.ReverseEndianness(ref AudioDataRelOffset);
            Utils.ReverseEndianness(ref AudioDataRelSize);
        }
    }
}