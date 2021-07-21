using System.Collections.Generic;
using LibHac.Fs;

namespace NxCore
{
    public abstract class BinaryRevolutionReader
    {
        public abstract IStorage Handle { get; }

        public abstract void Read();

        public uint ReadOffsetFromTable(uint tableStart, uint width, uint idx) {
            return Handle.ReadUInt32BigEndian(tableStart + width + width * idx);
        }

        public void ReadBlockInfoFromOffsetTable(uint tableStart, uint idx, uint width, out uint offsetStart,
            out uint offsetEnd) {
            uint offset = width + width * idx;
            // Console.WriteLine(offset);
            offsetStart = Handle.ReadUInt32BigEndian(tableStart + offset);
            // read four bytes to skip the enable flag since its always on
            offset += width;
            offsetEnd = Handle.ReadUInt32BigEndian(tableStart + offset);
        }

        public string ReadNullTermStringFromTable(uint offsetsRelative, uint tableOffset, uint idx) {
            ReadBlockInfoFromOffsetTable(offsetsRelative + tableOffset, idx, 4, out uint offsetFromTable,
                out uint offsetEnd);
            int bufferSize = (int)(offsetEnd - offsetFromTable);
            return Handle.ReadNullTerm(offsetsRelative + offsetFromTable, bufferSize);
        }

        public void MarshalFromTableOffset<T>(uint offsetsRelative, uint tableOffset, uint idx, out T t,
            Dictionary<uint, T>? cache = null)
            where T : struct, IEndianAwareUnmanagedType {
            if (cache != null && cache.ContainsKey(idx)) {
                t = cache[idx];
                return;
            }

            uint offsetFromTable = ReadOffsetFromTable(offsetsRelative + tableOffset, 8, idx);
            uint address = offsetsRelative + offsetFromTable;
            // Console.WriteLine($"{offsetsRelative:X8} + {offsetFromTable:X8}");
            DoMarshal(address, out t);
            if (cache != null) {
                cache[idx] = t;
            }
        }

        public void DoMarshal<T>(uint address, out T t)
            where T : struct, IEndianAwareUnmanagedType {
            // Console.WriteLine($"{address:X8}");
            Handle.ReadStruct(address, out t);
            t.FixEndian();
        }
    }
}