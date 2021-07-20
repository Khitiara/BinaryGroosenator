using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Brsar.Sound.Collection;
using Brsar.Sound.Data;
using NxCore;

namespace Brsar
{
    public partial class BrsarReader : BinaryRevolutionReader, IDisposable
    {
        public string WorkingDir { get; }
        public const uint BrsarHeaderMagic   = 0x52534152;
        public const uint SymbHeaderMagic    = 0x53594D42;
        public const uint InfoHeaderMagic    = 0x494E464F;
        public const uint FileHeaderMagic    = 0x46494C45;
        public const uint SectionOffsetShift = 0x8;

        public BrsarHeader Header;
        public SymbHeader  Symb;
        public InfoHeader  Info;
        public bool        FileIsBigEndian;
        public uint        FileNamesCount;
        public uint        FileCount;
        public uint        SoundDataCount;

        public readonly Dictionary<uint, CollectionEntry> CollectionCache;
        public readonly Dictionary<uint, LazySoundData>   SoundDataCache;
        public readonly MemoryMappedFile                  File;

        // _handle will usually be a MemoryMappedViewStream by my estimation
        public override MemoryMappedViewAccessor Handle { get; }

        public BrsarReader(string workingDir, MemoryMappedFile file) {
            WorkingDir = workingDir;
            Files = new FilesCollection(this);
            File = file;
            Handle = File.CreateViewAccessor();
            SoundDataCache = new Dictionary<uint, LazySoundData>();
            CollectionCache = new Dictionary<uint, CollectionEntry>();
        }

        public override void Read() {
            Handle.Read(0, out Header);
            FileIsBigEndian = Header.FixEndian();
            if (Header.Magic != BrsarHeaderMagic)
                throw new IOException("Corrupted BSAR header");

            DoMarshal(Header.SymbOffset, out Symb);
            if (Symb.Magic != SymbHeaderMagic)
                throw new IOException("Corrupted SYMB Header");

            FileNamesCount =
                BinaryPrimitives.ReverseEndianness(
                    Handle.ReadUInt32(Header.SymbOffset + Symb.FileNameTableOffset + SectionOffsetShift));
            FileCount = BinaryPrimitives.ReverseEndianness(
                Handle.ReadUInt32(Header.InfoOffset + SectionOffsetShift + Info.CollectionTableOffset));

            DoMarshal(Header.InfoOffset, out Info);
            if (Info.Magic != InfoHeaderMagic)
                throw new IOException("Corrupted INFO Header");


            SoundDataCount =
                BinaryPrimitives.ReverseEndianness(
                    Handle.ReadUInt32(Header.InfoOffset + SectionOffsetShift + Info.SoundDataOffset));
        }

        public void MarshalSoundData(uint idx, out LazySoundData data) {
            if (SoundDataCache.ContainsKey(idx)) {
                data = SoundDataCache[idx];
                return;
            }

            MarshalFromTableOffset(Header.InfoOffset + SectionOffsetShift, Info.SoundDataOffset, idx,
                out SoundDataEntry entry);
            data = entry.Type switch {
                SoundType.SEQ => new LazySeqData(this, entry),
                SoundType.STRM => new LazyStrmData(this, entry),
                SoundType.WAVE => new LazyWavData(this, entry),
                _ => throw new ArgumentOutOfRangeException()
            };

            SoundDataCache[idx] = data;
        }

        public void Dispose() {
            GC.SuppressFinalize(this);
            Handle.Dispose();
        }
    }
}