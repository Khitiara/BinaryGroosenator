using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace NxCore
{
    public static class Utils
    {
        public static bool MarshalRead<T>(this Stream stream, ref T output)
            where T : struct {
            return stream.Read(MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref output, 1))) == Unsafe.SizeOf<T>();
        }

        public static void ReverseEndianness(ref uint i) {
            i = BinaryPrimitives.ReverseEndianness(i);
        }

        public static void ReverseEndianness(ref int i) {
            i = BinaryPrimitives.ReverseEndianness(i);
        }

        public static void ReverseEndianness(ref ushort i) {
            i = BinaryPrimitives.ReverseEndianness(i);
        }

        public static string ReadNullTerm(this ReadOnlySpan<char> span, int offset) {
            ReadOnlySpan<char> slice = span[offset..];
            return slice[..slice.IndexOf('\0')].ToString();
        }

        public static string ReadNullTerm(this MemoryMappedViewAccessor stream, long address, int bufSize) {
            Span<byte> buffer = new byte[bufSize];
            return stream.ReadNullTerm(address, buffer);
        }

        public static string ReadNullTerm(this MemoryMappedViewAccessor stream, long address, Span<byte> buffer) {
            int b, i = 0;
            while ((b = stream.ReadByte(address + i)) > 0) {
                buffer[i++] = (byte)b;
            }

            return Encoding.ASCII.GetString(buffer[..i]);
        }
    }
}