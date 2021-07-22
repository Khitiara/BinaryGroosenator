using System.Runtime.InteropServices;
using NxCore;

namespace Dialogue.Msbt
{
    [StructLayout(LayoutKind.Explicit)]
    public struct Txt2Header : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint Magic;

        [FieldOffset(0x4)]
        public uint SectionSize;

        [FieldOffset(0x10)]
        public uint EntryCount;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref SectionSize);
            Utils.ReverseEndianness(ref EntryCount);
        }
    }
}