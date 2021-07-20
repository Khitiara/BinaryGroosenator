﻿using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.MemoryMappedFiles;
using NxCore;

namespace Brstm
{
    public partial class BrstmReader : BinaryRevolutionReader, IDisposable
    {
        public const uint                 FileTypeMagic = 0x5253544D;
        public const uint                 HeaderMagic   = 0x48454144;
        public const uint                 DataMagic     = 0x44415441;
        public       BrstmHeader          Header;
        public       HeadLeader           HeadLead;
        public       HeadChunk            HeaderChunk;
        public       byte                 TrackCount;
        public       TrackDescriptionType TrackType;
        public       TrackDescription[]   Tracks;
        public       byte                 ChannelCount;
        public       uint                 HeadOffset;
        public       uint                 DataOffset;
        public       uint                 DataLength;

        public short[,] AdpcmHsamples1;
        public short[,] AdpcmHsamples2;
        public short[,] AdpcmCoeffs;
        public bool IsBrwav => false;
        public override MemoryMappedViewAccessor Handle { get; }

        public BrstmReader(MemoryMappedViewAccessor handle) {
            Handle = handle;
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
            if (BinaryPrimitives.ReverseEndianness(Handle.ReadUInt32(DataOffset)) != DataMagic) {
                throw new IOException("Corrupt DATA header");
            }

            DataLength = BinaryPrimitives.ReverseEndianness(Handle.ReadUInt32(DataOffset + 0x4));
            DataOffset += 0x8;
        }

        public void Dispose() {
            Handle.Dispose();
        }
    }
}