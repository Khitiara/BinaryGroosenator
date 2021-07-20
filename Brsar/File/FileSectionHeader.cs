using System.Runtime.InteropServices;
using NxCore;

namespace Brsar.File
{
    [StructLayout(LayoutKind.Sequential, Size = 0x20)]
    public struct FileSectionHeader: IEndianAwareUnmanagedType
    {
        public uint Magic;
        public uint Size;
        public void FixEndian() {
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref Size);
        }
    }
}