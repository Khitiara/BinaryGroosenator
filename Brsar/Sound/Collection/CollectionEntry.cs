using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Collection
{
    [StructLayout(LayoutKind.Explicit, Size = 0x1C, Pack = 4)]
    public struct CollectionEntry : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x0)]
        public uint FileLengthNoAudio;
        [FieldOffset(0x4)]
        public uint AudioLength;
        [FieldOffset(0x8)]
        public int EntryNumber;
        [FieldOffset(0xC)]
        [MarshalAs(UnmanagedType.U1)]
        public bool EnableExternalFileNameOffset;
        [FieldOffset(0x10)]
        public uint ExternalFileNameOffset;
        [FieldOffset(0x14)]
        [MarshalAs(UnmanagedType.U1)]
        public bool EnableCollectionPositionTableOffset;
        [FieldOffset(0x18)]
        public uint CollectionPositionTableOffset;

        public void FixEndian() {
            Utils.ReverseEndianness(ref FileLengthNoAudio);
            Utils.ReverseEndianness(ref AudioLength);
            Utils.ReverseEndianness(ref EntryNumber);
            Utils.ReverseEndianness(ref ExternalFileNameOffset);
            Utils.ReverseEndianness(ref CollectionPositionTableOffset);
        }

        public override string ToString() {
            return $"{nameof(FileLengthNoAudio)}: {FileLengthNoAudio}, {nameof(AudioLength)}: {AudioLength}, {nameof(EntryNumber)}: {EntryNumber}, {nameof(EnableExternalFileNameOffset)}: {EnableExternalFileNameOffset}, {nameof(ExternalFileNameOffset)}: {ExternalFileNameOffset}, {nameof(EnableCollectionPositionTableOffset)}: {EnableCollectionPositionTableOffset}, {nameof(CollectionPositionTableOffset)}: {CollectionPositionTableOffset}";
        }
    }
}