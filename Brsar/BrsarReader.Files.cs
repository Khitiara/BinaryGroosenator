using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using Brsar.IO;
using Brsar.Sound.Collection;
using Brsar.Sound.Group;
using NxCore;

namespace Brsar
{
    public partial class BrsarReader
    {
        public class FilesCollection
        {
            private BrsarReader _reader;

            public FilesCollection(BrsarReader reader) {
                _reader = reader;
            }

            public IDataStorage this[uint fileId] => _reader._streams[(int)fileId].Value;
            public uint Count => _reader.FileCount;
        }

        private readonly List<Lazy<IDataStorage>> _streams = new();
        public readonly  FilesCollection          Files;

        public void LoadFiles() {
            uint infoOffset = Header.InfoOffset + SectionOffsetShift;
            for (int i = 0; i < FileCount; i++) {
                MarshalFromTableOffset(infoOffset, Info.CollectionTableOffset, (uint)i,
                    out CollectionEntry fileMeta, CollectionCache);
                uint fileGroupsCount =
                    BinaryPrimitives.ReverseEndianness(
                        Handle.ReadUInt32(infoOffset + fileMeta.CollectionPositionTableOffset));
                if (fileGroupsCount == 0) {
                    // External file, store a lazy filestream
                    string extFileName = Handle.ReadNullTerm(infoOffset + fileMeta.ExternalFileNameOffset, 0x30);
                    string fullPath = Path.Join(WorkingDir, extFileName);
                    _streams[i] = new Lazy<IDataStorage>(() =>
                        new ExtFileStorage(MemoryMappedFile.CreateFromFile(fullPath)));
                } else {
                    // Internal file, store a lazy memory-map view stream
                    MarshalFromTableOffset(infoOffset, fileMeta.CollectionPositionTableOffset, 0,
                        out CollectionPositionEntry entry);
                    MarshalFromTableOffset(infoOffset, Info.GroupTableOffset, entry.GroupIdx, out GroupDataEntry group);
                    MarshalFromTableOffset(infoOffset, group.SubsectionOffset, entry.IdxInGroup,
                        out GroupDataSubsection subsection);
                    _streams[i] = new Lazy<IDataStorage>(() =>
                        PartialDataStorage.PartialLoad(File.CreateViewAccessor(
                                group.MetadataFileOffset + subsection.MetadataRelOffset, subsection.MetadataRelSize),
                            File.CreateViewAccessor(group.AudioDataFileOffset + subsection.AudioDataRelOffset,
                                subsection.AudioDataRelSize)));
                }
            }
        }
    }
}