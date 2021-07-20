using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    public enum TrackDescriptionType : byte
    {
        SmashBrawl    = 0,
        NotSmashBrawl = 1
    }

    // Type-1 track description, AKA not-smash-brawl
    [StructLayout(LayoutKind.Explicit, Size = 0xC)]
    public struct TrackDescription : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public byte Volume;

        [FieldOffset(0x1)]
        public byte Panning;

        [FieldOffset(0x8)]
        public byte ChannelCount;

        [FieldOffset(0x9)]
        public byte LeftChannelId;

        [FieldOffset(0xA)]
        public byte RightChannelId;

        public void FixEndian() { }
    }
}