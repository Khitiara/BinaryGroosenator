using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using LibHac.Fs;
using NxCore;

namespace Brstm
{
    public partial class BrstmReader : BinaryRevolutionReader, IDisposable
    {
        public const uint     FileTypeMagic = 0x5253544D;
        public const uint     HeaderMagic   = 0x48454144;
        public const uint     DataMagic     = 0x44415441;
        public       short[,] AdpcmCoeffs;

        public short[,]             AdpcmHsamples1;
        public short[,]             AdpcmHsamples2;
        public byte                 ChannelCount;
        public uint                 DataLength;
        public uint                 DataOffset;
        public BrstmHeader          Header;
        public HeadChunk            HeaderChunk;
        public HeadLeader           HeadLead;
        public uint                 HeadOffset;
        public byte                 TrackCount;
        public TrackDescription[]   Tracks;
        public TrackDescriptionType TrackType;

        public BrstmReader(IStorage handle) {
            Handle = handle;
        }

        public bool IsBrwav => false;
        public override IStorage Handle { get; }

        public void Dispose() {
            Handle.Dispose();
        }

        public override void Read() {
            DoMarshal(0, out Header);
            if (Header.Magic != FileTypeMagic)
                throw new IOException("Corrupt BRSTM Header");

            DoMarshal(Header.HeadOffset, out HeadLead);
            if (HeadLead.Magic != HeaderMagic)
                throw new IOException("Corrupt HEAD chunk");

            HeadOffset = Header.HeadOffset + 0x8;
            DoMarshal(HeadOffset + HeadLead.Chunk1Offset, out HeaderChunk);
            if (HeaderChunk.Codec == BrstmCodec.ADPCM4) {
                throw new NotImplementedException(); // ADPCM not yet implemented 
            } else if (HeaderChunk.Codec > BrstmCodec.ADPCM4) {
                throw new NotSupportedException("Unsupported BRSTM Codec");
            }

            TrackCount = Handle.ReadByte(HeadOffset + HeadLead.TrackDescTableOffset + 0x0);
            TrackType = (TrackDescriptionType)Handle.ReadByte(HeadOffset + HeadLead.TrackDescTableOffset +
                                                              0x1);

            if (TrackCount > 8) {
                throw new NotSupportedException("Too many tracks!");
            }

            if (TrackType == TrackDescriptionType.SmashBrawl) {
                throw new NotSupportedException("Smash Brawl not supported");
            }

            Tracks = new TrackDescription[TrackCount];
            for (uint i = 0; i < TrackCount; i++) {
                MarshalFromTableOffset(HeadOffset, HeadLead.TrackDescTableOffset, i, out Tracks[i]);
            }

            ChannelCount = Handle.ReadByte(HeadOffset + HeadLead.ChannelInfoOffset);
            if (ChannelCount > 16) {
                throw new NotSupportedException("Too many channels!");
            }

            DataOffset = Header.DataOffset;
            if (Handle.ReadUInt32BigEndian(DataOffset) != DataMagic) {
                throw new IOException("Corrupt DATA header");
            }

            DataLength = Handle.ReadUInt32BigEndian(DataOffset + 0x4);
            DataOffset += 0x8;
        }
    }
}