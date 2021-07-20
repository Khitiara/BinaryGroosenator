using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Data
{
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct RSWDSubsectionTwo : IEndianAwareUnmanagedType, ISoundDataSubsection
    {
        [FieldOffset(0x0)]
        public uint SoundDataNode;

        [FieldOffset(0x7)]
        [MarshalAs(UnmanagedType.I1)]
        public bool RWSDAllocTrack;

        [FieldOffset(0x8)]
        public byte Priority;

        public void FixEndian() {
            Utils.ReverseEndianness(ref SoundDataNode);
        }
    }
}