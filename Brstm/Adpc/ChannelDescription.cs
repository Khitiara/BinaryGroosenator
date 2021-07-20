using System.Runtime.InteropServices;
using NxCore;

namespace Brstm
{
    [StructLayout(LayoutKind.Explicit, Size = 0x38)]
    public struct ChannelDescription : IEndianAwareUnmanagedType
    {
        [FieldOffset(0x08)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 0x10)]
        public ushort[] Coeffs;

        [FieldOffset(0x28)]
        public ushort Gain;

        [FieldOffset(0x2A)]
        public ushort InitialPredictor;

        [FieldOffset(0x2C)]
        public ushort HistorySample1;

        [FieldOffset(0x2E)]
        public ushort HistorySample2;

        [FieldOffset(0x30)]
        public ushort LoopPredictor;

        [FieldOffset(0x32)]
        public ushort LoopHistorySample1;

        [FieldOffset(0x34)]
        public ushort LoopHistorySample2;

        public void FixEndian() {
            for (int i = 0; i < Coeffs.Length; i++) {
                Utils.ReverseEndianness(ref Coeffs[i]);
            }

            Utils.ReverseEndianness(ref Gain);
            Utils.ReverseEndianness(ref InitialPredictor);
            Utils.ReverseEndianness(ref HistorySample1);
            Utils.ReverseEndianness(ref HistorySample2);
            Utils.ReverseEndianness(ref LoopPredictor);
            Utils.ReverseEndianness(ref LoopHistorySample1);
            Utils.ReverseEndianness(ref LoopHistorySample2);
        }
    }
}