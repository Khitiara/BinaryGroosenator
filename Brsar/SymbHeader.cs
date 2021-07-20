using System.Runtime.InteropServices;
using NxCore;

namespace Brsar
{
    [StructLayout(LayoutKind.Sequential)]
    public struct SymbHeader : IEndianAwareUnmanagedType
    {
        public uint Magic;
        public uint Size;
        public uint FileNameTableOffset;
        public uint MaskTable1Offset;
        public uint MaskTable2Offset;
        public uint MaskTable3Offset;
        public uint MaskTable4Offset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref Size);
            Utils.ReverseEndianness(ref FileNameTableOffset);
            Utils.ReverseEndianness(ref MaskTable1Offset);
            Utils.ReverseEndianness(ref MaskTable2Offset);
            Utils.ReverseEndianness(ref MaskTable3Offset);
            Utils.ReverseEndianness(ref MaskTable4Offset);
        }
    }
}