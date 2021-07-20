using System;
using System.Runtime.InteropServices;
using Brsar.Sound.Collection;
using Brsar.Sound.Data;
using NxCore;

namespace Brsar
{
    [StructLayout(LayoutKind.Explicit, Size = 0x14)]
    public struct SoundDataSubsection
    {
        [FieldOffset(0x0)]
        public RSWDSubsectionTwo Rswd;

        [FieldOffset(0x0)]
        public RSTMSubsectionTwo Rstm;

        [FieldOffset(0x0)]
        public RSEQSubsectionTwo Rseq;
    }

    public class LazySoundData
    {
        public SoundDataEntry Entry { get; }
        public string Filename => _filename.Value;
        public SoundDataSubsection TypedSubsection => _typedSubsection.Value;

        public RSWDSubsectionTwo RSWD {
            get {
                if (Entry.Type == SoundType.WAVE)
                    return TypedSubsection.Rswd;
                throw new InvalidOperationException();
            }
        }

        public RSEQSubsectionTwo RSEQ {
            get {
                if (Entry.Type == SoundType.SEQ)
                    return TypedSubsection.Rseq;
                throw new InvalidOperationException();
            }
        }

        public RSTMSubsectionTwo RSTM {
            get {
                if (Entry.Type == SoundType.STRM)
                    return TypedSubsection.Rstm;
                throw new InvalidOperationException();
            }
        }

        public CollectionEntry Collection => _collection.Value;

        private readonly Lazy<string>                                        _filename;
        private readonly Lazy<SoundDataSubsection>                           _typedSubsection;
        private readonly Lazy<CollectionEntry>                               _collection;
        private readonly Lazy<string?>                                       _collectionExternalFilePath;
        private readonly Lazy<LazyBrsarOffsetTable<CollectionPositionEntry>> _collectionPositionTable;
        private readonly Lazy<Sound3DParams>                                 _subsectionThree;
        public string? CollectionExternalFilePath => _collectionExternalFilePath.Value;
        public LazyBrsarOffsetTable<CollectionPositionEntry> CollectionPositionTable => _collectionPositionTable.Value;
        public Sound3DParams SubsectionThree => _subsectionThree.Value;

        private string GetFileName() => Reader.ReadNullTermStringFromTable(
            Reader.Header.SymbOffset + BrsarReader.SectionOffsetShift,
            Reader.Symb.FileNameTableOffset, Entry.FileNameIdx);

        private CollectionEntry LoadCollection() {
            CollectionEntry value = default;
            Reader.MarshalFromTableOffset(Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift,
                Reader.Info.CollectionTableOffset, Entry.FileCollectionIdx, out value, Reader.CollectionCache);
            return value;
        }

        private SoundDataSubsection LoadSubsection() {
            SoundDataSubsection subsection = default;
            switch (Entry.Type) {
                case SoundType.SEQ:
                    Reader.DoMarshal(
                        Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift + Entry.SubsectionTwoOffset,
                        out subsection.Rseq);
                    break;
                case SoundType.STRM:
                    Reader.DoMarshal(
                        Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift + Entry.SubsectionTwoOffset,
                        out subsection.Rstm);
                    break;
                case SoundType.WAVE:
                    Reader.DoMarshal(
                        Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift + Entry.SubsectionTwoOffset,
                        out subsection.Rswd);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return subsection;
        }

        protected readonly BrsarReader Reader;

        public LazySoundData(BrsarReader reader, SoundDataEntry entry) {
            Entry = entry;
            Reader = reader;
            _collection = new Lazy<CollectionEntry>(LoadCollection);
            _typedSubsection = new Lazy<SoundDataSubsection>(LoadSubsection);
            _filename = new Lazy<string>(GetFileName);
            _collectionExternalFilePath = new Lazy<string?>(GetCollectionExternalPath);
            _collectionPositionTable = new Lazy<LazyBrsarOffsetTable<CollectionPositionEntry>>(GetCollectionPosTable);
            _subsectionThree = new Lazy<Sound3DParams>(SubsectionThreeGetter);
        }

        private Sound3DParams SubsectionThreeGetter() {
            Reader.DoMarshal(Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift + Entry.SubsectionThreeOffset,
                out Sound3DParams value);
            return value;
        }

        private LazyBrsarOffsetTable<CollectionPositionEntry> GetCollectionPosTable() {
            return new LazyBrsarOffsetTable<CollectionPositionEntry>(Reader,
                Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift,
                Collection.CollectionPositionTableOffset);
        }

        public override string ToString() {
            return $"FileName:\t{Filename}\n" +
                   $"SoundType:\t{Entry.Type.ToString()}\n" +
                   $"CollIdx:\t{Entry.FileCollectionIdx:X8}\n" +
                   $"ExtFilePath:\t{(Collection.EnableExternalFileNameOffset ? CollectionExternalFilePath : "null")}\n" +
                   $"PosTableSize:\t{(Collection.EnableCollectionPositionTableOffset ? CollectionPositionTable.Count : 0):X8}\n" +
                   $"Subsection:\n{WriteSubsection()}\n" /* +
                   $"\nRaw:\tEntry({Entry.ToString()})\n" +
                   $"\tCollection({Collection.ToString()})\n"*/ +
                   $"3DParamFlags:\t{SubsectionThree.Sound3DParamFlags:X8}\n" +
                   $"DecayCurve:\t{SubsectionThree.DecayCurve:X2}\n" +
                   $"DecayRatio:\t{SubsectionThree.DecayRatio:X2}\n";
        }

        private string WriteSubsection() {
            return Entry.Type switch {
                SoundType.SEQ => $"\tSeqLabelEntry:\t{RSEQ.SeqLabelEntry:X8}\n" +
                                 $"\tSoundBankEntry:\t{RSEQ.SoundBankEntry:X8}\n" +
                                 $"\tRSEQAllocTrack:\t{RSEQ.RSEQAllocTrack:X2}\n" +
                                 $"\tSeqChannelPriority:\t{RSEQ.SeqChannelPriority:X2}",
                SoundType.STRM => $"\tStartPosition:\t{RSTM.StartPosition:X8}\n" +
                                  $"\tAllocChannelCount:\t{RSTM.AllocChannelCount:X4}\n" +
                                  $"\tRSTMAllocTrack:\t{RSTM.RSTMAllocTrack:X4}",
                SoundType.WAVE => $"\tSoundDataNode:\t{RSWD.SoundDataNode:X8}\n" +
                                  $"\tRSWDAllocTrack:\t{RSWD.RWSDAllocTrack}\n" +
                                  $"\tPriority:\t{RSWD.Priority:X2}",
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private string? GetCollectionExternalPath() {
            if (!Collection.EnableCollectionPositionTableOffset || Collection.ExternalFileNameOffset == 0)
                return null;
            uint offset = Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift +
                          Collection.ExternalFileNameOffset;
            return Reader.Handle.ReadNullTerm(offset, 0x30);
        }
    }
}