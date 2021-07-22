using System.Runtime.InteropServices;
using NxCore;

namespace Dialogue.Msbt
{
    [StructLayout(LayoutKind.Sequential, Size = 0x8)]
    public struct Lbl1Entry : IEndianAwareUnmanagedType
    {
        public uint PairCount;
        public uint Offset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref PairCount);
            Utils.ReverseEndianness(ref Offset);
        }
    }
}