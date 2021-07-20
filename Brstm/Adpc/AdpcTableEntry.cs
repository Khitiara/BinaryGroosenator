using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    [StructLayout(LayoutKind.Sequential)]
    public struct AdpcTableEntry : IEndianAwareUnmanagedType
    {
        public ushort HistorySample1;
        public ushort HistorySample2;

        public void FixEndian() {
            Utils.ReverseEndianness(ref HistorySample1);
            Utils.ReverseEndianness(ref HistorySample2);
        }
    }
}