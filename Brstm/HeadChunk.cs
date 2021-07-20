using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    public enum BrstmCodec : byte
    {
        PCM8   = 0,
        PCM16  = 1,
        ADPCM4 = 2
    }

    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 0x34)]
    public struct HeadChunk : IEndianAwareUnmanagedType
    {
        public BrstmCodec Codec;

        [MarshalAs(UnmanagedType.U1)]
        public bool LoopFlag;

        public byte   ChannelCount;
        public ushort SampleRate;
        public uint   LoopStartSampleIdx;
        public uint   SampleCount;
        public uint   DataOffsetAbsolute;
        public uint   TotalInterlacedBlockCount;
        public uint   BlockSizeBytes;
        public uint   SamplesPerBlock;
        public uint   FinalBlockSizeNoPad;
        public uint   FinalBlockSampleCount;
        public uint   FinalBlockSizeWithPad;
        public uint   ADPCSamplesPerEntry;
        public uint   ADPCBytesPerEntry;

        public void FixEndian() {
            Utils.ReverseEndianness(ref SampleRate);
            Utils.ReverseEndianness(ref LoopStartSampleIdx);
            Utils.ReverseEndianness(ref SampleCount);
            Utils.ReverseEndianness(ref DataOffsetAbsolute);
            Utils.ReverseEndianness(ref TotalInterlacedBlockCount);
            Utils.ReverseEndianness(ref BlockSizeBytes);
            Utils.ReverseEndianness(ref SamplesPerBlock);
            Utils.ReverseEndianness(ref FinalBlockSizeNoPad);
            Utils.ReverseEndianness(ref FinalBlockSampleCount);
            Utils.ReverseEndianness(ref FinalBlockSizeWithPad);
            Utils.ReverseEndianness(ref ADPCSamplesPerEntry);
            Utils.ReverseEndianness(ref ADPCBytesPerEntry);
        }
    }
}