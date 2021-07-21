using System;
using System.Collections.Generic;
using System.IO;
using Brsar.IO;
using Brsar.Sound.Collection;
using Brsar.Sound.Group;
using LibHac.Fs;
using LibHac.FsSystem;
using NxCore;

namespace Brsar
{
    public partial class BrsarReader
    {
        private readonly List<Lazy<IStorage>> _streams = new();
        public readonly  FilesCollection      Files;

        public void LoadFiles() {
            uint infoOffset = Header.InfoOffset + SectionOffsetShift;
            for (int i = 0; i < FileCount; i++) {
                MarshalFromTableOffset(infoOffset, Info.CollectionTableOffset, (uint)i,
                    out CollectionEntry fileMeta, CollectionCache);
                uint fileGroupsCount =
                    Handle.ReadUInt32BigEndian(infoOffset + fileMeta.CollectionPositionTableOffset);
                if (fileGroupsCount == 0) {
                    // External file, store a lazy filestream
                    string extFileName = Handle.ReadNullTerm(infoOffset + fileMeta.ExternalFileNameOffset, 0x30);
                    string fullPath = Path.Join(WorkingDir, extFileName);
                    _streams[i] = new Lazy<IStorage>(() => new LocalStorage(fullPath, FileAccess.Read));
                } else {
                    // Internal file, store a lazy memory-map view stream
                    MarshalFromTableOffset(infoOffset, fileMeta.CollectionPositionTableOffset, 0,
                        out CollectionPositionEntry entry);
                    MarshalFromTableOffset(infoOffset, Info.GroupTableOffset, entry.GroupIdx, out GroupDataEntry group);
                    MarshalFromTableOffset(infoOffset, group.SubsectionOffset, entry.IdxInGroup,
                        out GroupDataSubsection subsection);
                    _streams[i] = new Lazy<IStorage>(() =>
                        PartialDataStorage.PartialLoad(
                            Handle.Slice(group.MetadataFileOffset + subsection.MetadataRelOffset,
                                subsection.MetadataRelSize),
                            Handle.Slice(group.AudioDataFileOffset + subsection.AudioDataRelOffset,
                                subsection.AudioDataRelSize)));
                }
            }
        }

        public class FilesCollection
        {
            private BrsarReader _reader;

            public FilesCollection(BrsarReader reader) {
                _reader = reader;
            }

            public IStorage this[uint fileId] => _reader._streams[(int)fileId].Value;
            public uint Count => _reader.FileCount;
        }
    }
}