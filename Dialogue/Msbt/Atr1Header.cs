using System.Runtime.InteropServices;
using NxCore;

namespace Dialogue.Msbt
{
    [StructLayout(LayoutKind.Explicit, Size = 0x18)]
    public struct Atr1Header : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint Magic;

        [FieldOffset(0x4)]
        public uint SectionSize;

        [FieldOffset(0x10)]
        public uint EntryCount;

        [FieldOffset(0x14)]
        public uint EntrySize;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref SectionSize);
            Utils.ReverseEndianness(ref EntryCount);
            Utils.ReverseEndianness(ref EntrySize);
        }
    }
}