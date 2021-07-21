using System;
using System.Collections.Generic;
using System.IO;
using Brsar.Sound.Collection;
using Brsar.Sound.Data;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using NxCore;

namespace Brsar
{
    public partial class BrsarReader : BinaryRevolutionReader
    {
        public const uint BrsarHeaderMagic   = 0x52534152;
        public const uint SymbHeaderMagic    = 0x53594D42;
        public const uint InfoHeaderMagic    = 0x494E464F;
        public const uint FileHeaderMagic    = 0x46494C45;
        public const uint SectionOffsetShift = 0x8;

        public readonly Dictionary<uint, CollectionEntry> CollectionCache;
        public readonly IFile                             File;
        public readonly Dictionary<uint, LazySoundData>   SoundDataCache;
        public          uint                              FileCount;
        public          bool                              FileIsBigEndian;
        public          uint                              FileNamesCount;

        public BrsarHeader Header;
        public InfoHeader  Info;
        public uint        SoundDataCount;
        public SymbHeader  Symb;

        public BrsarReader(IFileSystem workingFs, string workingDir, IFile file) {
            WorkingFs = workingFs;
            WorkingDir = workingDir;
            Files = new FilesCollection(this);
            File = file;
            Handle = File.AsStorage();
            SoundDataCache = new Dictionary<uint, LazySoundData>();
            CollectionCache = new Dictionary<uint, CollectionEntry>();
        }

        public IFileSystem WorkingFs { get; }
        public string WorkingDir { get; }

        // _handle will usually be a MemoryMappedViewStream by my estimation
        public override IStorage Handle { get; }

        public override void Read() {
            Handle.ReadStruct(0, out Header);
            FileIsBigEndian = Header.FixEndian();
            if (Header.Magic != BrsarHeaderMagic)
                throw new IOException("Corrupted BSAR header");

            DoMarshal(Header.SymbOffset, out Symb);
            if (Symb.Magic != SymbHeaderMagic)
                throw new IOException("Corrupted SYMB Header");

            FileNamesCount =
                Handle.ReadUInt32BigEndian(Header.SymbOffset + Symb.FileNameTableOffset + SectionOffsetShift);
            FileCount = Handle.ReadUInt32BigEndian(Header.InfoOffset + SectionOffsetShift + Info.CollectionTableOffset);

            DoMarshal(Header.InfoOffset, out Info);
            if (Info.Magic != InfoHeaderMagic)
                throw new IOException("Corrupted INFO Header");


            SoundDataCount =
                Handle.ReadUInt32BigEndian(Header.InfoOffset + SectionOffsetShift + Info.SoundDataOffset);
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
    }
}