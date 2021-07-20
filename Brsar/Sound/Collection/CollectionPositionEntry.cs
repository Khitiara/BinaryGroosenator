using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Collection
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CollectionPositionEntry : IEndianAwareUnmanagedType
    {
        public uint GroupIdx;
        public uint IdxInGroup;

        public void FixEndian() {
            Utils.ReverseEndianness(ref GroupIdx);
            Utils.ReverseEndianness(ref IdxInGroup);
        }
    }
}