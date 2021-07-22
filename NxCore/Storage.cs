using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LibHac;
using LibHac.Fs;

namespace NxCore
{
    public static class Storage
    {
        public static byte ReadByte(this IStorage storage, long position) {
            byte b = 0;
            storage.Read(position, MemoryMarshal.AsBytes(MemoryMarshal.CreateSpan(ref b, 1))).ThrowIfFailure();
            return b;
        }

        public static uint ReadUInt32BigEndian(this IStorage storage, long position) {
            Span<byte> span = new byte[sizeof(uint)];
            storage.Read(position, span).ThrowIfFailure();
            return BinaryPrimitives.ReadUInt32BigEndian(span);
        }

        public static Result ReadStruct<T>(this IStorage storage, long position, out T t)
            where T : struct {
            Span<byte> span = new byte[Unsafe.SizeOf<T>()];
            Result result = storage.Read(position, span);
            t = MemoryMarshal.Read<T>(span);
            return result;
        }

        public static string ReadNullTerm(this IStorage stream, long address, int bufSize) {
            Span<byte> buffer = new byte[bufSize];
            return stream.ReadNullTerm(address, buffer);
        }

        public static string ReadNullTerm(this IStorage stream, long address, Span<byte> buffer) {
            int b, i = 0;
            while ((b = stream.ReadByte(address + i)) > 0) {
                buffer[i++] = (byte)b;
            }

            return Encoding.ASCII.GetString(buffer[..i]);
        }
    }
}