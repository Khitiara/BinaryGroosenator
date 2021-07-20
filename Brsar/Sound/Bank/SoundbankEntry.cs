using NxCore;

namespace Brsar.Sound.Bank
{
    public struct SoundbankEntry : IEndianAwareUnmanagedType
    {
        public uint FileNameIdx;
        public uint FileCollectionIdx;
        public uint BankIdx;

        public void FixEndian() {
            Utils.ReverseEndianness(ref FileNameIdx);
            Utils.ReverseEndianness(ref FileCollectionIdx);
            Utils.ReverseEndianness(ref BankIdx);
        }
    }
}