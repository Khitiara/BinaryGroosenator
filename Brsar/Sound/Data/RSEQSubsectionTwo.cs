using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Data
{
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct RSEQSubsectionTwo : IEndianAwareUnmanagedType, ISoundDataSubsection
    {
        [FieldOffset(0x0)]
        public uint SeqLabelEntry;

        [FieldOffset(0x4)]
        public uint SoundBankEntry;

        [FieldOffset(0xB)]
        public byte RSEQAllocTrack;

        [FieldOffset(0xC)]
        public byte SeqChannelPriority;

        [FieldOffset(0xD)]
        public byte ReleasePriorityFix;

        public void FixEndian() {
            Utils.ReverseEndianness(ref SeqLabelEntry);
            Utils.ReverseEndianness(ref SoundBankEntry);
        }
    }
}