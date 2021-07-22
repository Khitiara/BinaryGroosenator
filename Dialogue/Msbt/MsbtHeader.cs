using System.IO;
using System.Runtime.InteropServices;
using NxCore;

namespace Dialogue.Msbt
{
    [StructLayout(LayoutKind.Explicit, Size = 0x20)]
    public struct MsbtHeader : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public ulong Magic;

        [FieldOffset(0x8)]
        public ushort Bom;

        [FieldOffset(0x12)]
        public uint FileSize;

        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref Bom);
            Utils.ReverseEndianness(ref FileSize);
            if (Bom != 0xFEFF)
                throw new InvalidDataException("Invalid Byte-order mark");
        }
    }
}