using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.Data
{
    [StructLayout(LayoutKind.Sequential, Size = 0xC)]
    public struct Sound3DParams : IEndianAwareUnmanagedType
    {
        public uint Sound3DParamFlags;
        public byte DecayCurve;
        public byte DecayRatio;
        public byte DopplerFactor;
        public void FixEndian() {
            Utils.ReverseEndianness(ref Sound3DParamFlags);
        }
    }
}