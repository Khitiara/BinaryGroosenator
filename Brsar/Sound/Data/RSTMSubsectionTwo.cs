using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Data
{
    [StructLayout(LayoutKind.Sequential, Size = 0x14)]
    public struct RSTMSubsectionTwo : IEndianAwareUnmanagedType, ISoundDataSubsection
    {
        public uint   StartPosition;
        public ushort AllocChannelCount;
        public ushort RSTMAllocTrack;

        public void FixEndian() {
            Utils.ReverseEndianness(ref StartPosition);
            Utils.ReverseEndianness(ref AllocChannelCount);
            Utils.ReverseEndianness(ref RSTMAllocTrack);
        }
    }
}