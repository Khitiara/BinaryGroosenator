using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Data
{
    [StructLayout(LayoutKind.Explicit, Pack = 1, Size = 0x2C)]
    public struct SoundDataEntry : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint FileNameIdx;

        [FieldOffset(0x4)]
        public uint FileCollectionIdx;

        [FieldOffset(0x8)]
        public uint PlayerId;

        [FieldOffset(0x10)]
        public uint SubsectionThreeOffset;

        [FieldOffset(0x14)]
        public byte Volume;

        [FieldOffset(0x15)]
        public byte PlayerPriority;

        [FieldOffset(0x16)]
        public SoundType Type;

        [FieldOffset(0x17)]
        public byte RemoteFilter;

        [FieldOffset(0x1C)]
        public uint SubsectionTwoOffset;

        [FieldOffset(0x20)]
        public uint UserParam1;

        [FieldOffset(0x24)]
        public uint UserParam2;

        [FieldOffset(0x28)]
        public PanMode PanMode;

        [FieldOffset(0x29)]
        public PanCurve PanCurve;

        [FieldOffset(0x2A)]
        public byte ActorPlayerID;

        public void FixEndian() {
            Utils.ReverseEndianness(ref FileNameIdx);
            Utils.ReverseEndianness(ref FileCollectionIdx);
            Utils.ReverseEndianness(ref PlayerId);
            Utils.ReverseEndianness(ref SubsectionThreeOffset);
            Utils.ReverseEndianness(ref SubsectionTwoOffset);
            Utils.ReverseEndianness(ref UserParam1);
            Utils.ReverseEndianness(ref UserParam2);
        }

        public override string ToString() {
            return $"{nameof(FileNameIdx)}: {FileNameIdx}, {nameof(FileCollectionIdx)}: {FileCollectionIdx}, {nameof(PlayerId)}: {PlayerId}, {nameof(SubsectionThreeOffset)}: {SubsectionThreeOffset}, {nameof(Volume)}: {Volume}, {nameof(PlayerPriority)}: {PlayerPriority}, {nameof(Type)}: {Type}, {nameof(RemoteFilter)}: {RemoteFilter}, {nameof(SubsectionTwoOffset)}: {SubsectionTwoOffset}, {nameof(UserParam1)}: {UserParam1}, {nameof(UserParam2)}: {UserParam2}, {nameof(PanMode)}: {PanMode}, {nameof(PanCurve)}: {PanCurve}, {nameof(ActorPlayerID)}: {ActorPlayerID}";
        }
    }
}