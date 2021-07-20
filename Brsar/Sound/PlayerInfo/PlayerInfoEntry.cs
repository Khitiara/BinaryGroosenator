using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.PlayerInfo
{
    [StructLayout(LayoutKind.Sequential, Size = 0x10)]
    public struct PlayerInfoEntry : IEndianAwareUnmanagedType
    {
        public uint FileNameIdx;
        public byte PlayableSoundCount;
        public void FixEndian() {
            Utils.ReverseEndianness(ref FileNameIdx);
        }
    }
}