using System;
using Brsar.Sound.Bank;
using Brsar.Sound.Collection;
using Brsar.Sound.Data;
using NxCore;

namespace Brsar
{
    public class LazySeqData : LazySoundData
    {
        internal LazySeqData(BrsarReader reader, SoundDataEntry entry) : base(reader, entry) {
            _soundbank = new Lazy<SoundbankEntry>(LoadSoundbank);
            _bankCollection = new Lazy<CollectionEntry>(LoadCollection);
            _bankFilename = new Lazy<string>(GetBankFilename);
            _bankCollectionExtFileName = new Lazy<string?>(GetBankCollectionExternalPath);
            _bankCollectionPositionTable =
                new Lazy<LazyBrsarOffsetTable<CollectionPositionEntry>>(GetBankCollectionPosTable);
        }

        private LazyBrsarOffsetTable<CollectionPositionEntry> GetBankCollectionPosTable() {
            return new LazyBrsarOffsetTable<CollectionPositionEntry>(Reader,
                Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift,
                BankCollection.CollectionPositionTableOffset);
        }

        private CollectionEntry LoadCollection() {
            uint idx = Soundbank.FileCollectionIdx;
            Reader.MarshalFromTableOffset(Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift,
                Reader.Info.CollectionTableOffset, idx, out CollectionEntry value, Reader.CollectionCache);
            return value;
        }

        private SoundbankEntry LoadSoundbank() {
            uint idx = RSEQ.SoundBankEntry;
            Console.WriteLine($"Loading soundbank {idx}");
            Reader.MarshalFromTableOffset(Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift,
                Reader.Info.SoundbankOffset, idx, out SoundbankEntry value);
            return value;
        }

        private string GetBankFilename() =>
            Reader.ReadNullTermStringFromTable(Reader.Header.SymbOffset + BrsarReader.SectionOffsetShift,
                Reader.Symb.FileNameTableOffset, Soundbank.FileNameIdx);

        private string? GetBankCollectionExternalPath() {
            if (!BankCollection.EnableCollectionPositionTableOffset || BankCollection.ExternalFileNameOffset == 0)
                return null;
            uint offset = Reader.Header.InfoOffset + BrsarReader.SectionOffsetShift +
                          BankCollection.ExternalFileNameOffset;
            return Reader.Handle.ReadNullTerm(offset, 0x30);
        }

        private readonly Lazy<SoundbankEntry>                                _soundbank;
        private readonly Lazy<CollectionEntry>                               _bankCollection;
        private readonly Lazy<string>                                        _bankFilename;
        private readonly Lazy<string?>                                       _bankCollectionExtFileName;
        private readonly Lazy<LazyBrsarOffsetTable<CollectionPositionEntry>> _bankCollectionPositionTable;

        public SoundbankEntry Soundbank => _soundbank.Value;
        public CollectionEntry BankCollection => _bankCollection.Value;
        public string BankFilename => _bankFilename.Value;
        public string? BankCollectionExtFileName => _bankCollectionExtFileName.Value;

        public LazyBrsarOffsetTable<CollectionPositionEntry> BankCollectionPositionTable =>
            _bankCollectionPositionTable.Value;

        public override string ToString() {
            return base.ToString() +
                   $"Soundbank File:\t{BankFilename}\n" +
                   $"Bank Coll Idx:\t{Soundbank.FileCollectionIdx}\n" +
                   $"BankCollFile:\t{(BankCollection.EnableExternalFileNameOffset ? BankCollectionExtFileName : "null")}\n" +
                   $"BankCollLen:\t{(BankCollection.EnableCollectionPositionTableOffset ? BankCollectionPositionTable.Count : 0)}\n";
        }
    }
}