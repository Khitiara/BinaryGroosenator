using System.IO;
using System.Runtime.InteropServices;
using NxCore;

namespace Brsar
{
    [StructLayout(LayoutKind.Sequential, Size = 0x40)]
    public struct BrsarHeader
    {
        public uint   Magic;
        public ushort Bom;
        public ushort Version;
        public uint   FileLength;
        public ushort HeaderLength;
        public ushort NumSections;
        public uint   SymbOffset;
        public uint   SymbSize;
        public uint   InfoOffset;
        public uint   InfoSize;
        public uint   FileDataOffset;
        public uint   FileDataSize;

        public bool FixEndian() {
            if (Bom == 0xFEFFu)
                return false;
            if (Bom != 0xFFFEu)
                throw new IOException("Invalid byte order mark");
            Utils.ReverseEndianness(ref Magic);
            Utils.ReverseEndianness(ref Bom);
            Utils.ReverseEndianness(ref Version);
            Utils.ReverseEndianness(ref FileLength);
            Utils.ReverseEndianness(ref HeaderLength);
            Utils.ReverseEndianness(ref NumSections);
            Utils.ReverseEndianness(ref SymbOffset);
            Utils.ReverseEndianness(ref SymbSize);
            Utils.ReverseEndianness(ref InfoOffset);
            Utils.ReverseEndianness(ref InfoSize);
            Utils.ReverseEndianness(ref FileDataOffset);
            Utils.ReverseEndianness(ref FileDataSize);
            return true;
        }
    }
}