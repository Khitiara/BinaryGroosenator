using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.Sound.SoundCount
{
    [StructLayout(LayoutKind.Sequential, Size = 0x1E)]
    public struct SoundCountTable : IEndianAwareUnmanagedType
    {
        public ushort SeqSoundCount;
        public ushort SeqTrackCount;
        public ushort StrmSoundCount;
        public ushort StrmTrackCount;
        public ushort WaveSoundCount;
        public ushort WaveTrackCount;
        public void FixEndian() {
            Utils.ReverseEndianness(ref SeqSoundCount);
            Utils.ReverseEndianness(ref SeqTrackCount);
            Utils.ReverseEndianness(ref StrmSoundCount);
            Utils.ReverseEndianness(ref StrmTrackCount);
            Utils.ReverseEndianness(ref WaveSoundCount);
            Utils.ReverseEndianness(ref WaveTrackCount);
        }
    }
}