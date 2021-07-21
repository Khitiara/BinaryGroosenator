using System;
using System.Buffers.Binary;

namespace Brstm
{
    public partial class BrstmReader
    {
        public BrstmInterleavingWaveProvider WriteTrack(int trackId) {
            TrackDescription description = Tracks[trackId];
            short[,] samples = new short[2, HeaderChunk.SampleCount];
            if (description.RightChannelId == 0x00) {
                // Mono track
                samples = new short[1, HeaderChunk.SampleCount];
                WriteChannel(description.LeftChannelId, 0, samples);
            } else {
                WriteChannel(description.LeftChannelId, 0, samples);
                WriteChannel(description.RightChannelId, 1, samples);
            }

            return new BrstmInterleavingWaveProvider(HeaderChunk, description, samples);
        }

        private void WriteChannel(int c, int outChannel, short[,] samples) {
            for (int b = 0; b < HeaderChunk.TotalInterlacedBlockCount; b++) {
                DecodeBlock(b, c, outChannel, samples);
            }
        }

        private void DecodeBlock(int b, int c, int c1, short[,] pcmSamples) {
            uint curBlockSize = b == HeaderChunk.TotalInterlacedBlockCount - 1
                ? HeaderChunk.FinalBlockSizeNoPad
                : HeaderChunk.BlockSizeBytes;
            uint curBlockSamples = b == HeaderChunk.TotalInterlacedBlockCount - 1
                ? HeaderChunk.FinalBlockSampleCount
                : HeaderChunk.SamplesPerBlock;
            ulong outputPos = (ulong)(HeaderChunk.SamplesPerBlock * b);

            long position;
            if (!IsBrwav) {
                position = HeaderChunk.BlockSizeBytes * c + b * HeaderChunk.BlockSizeBytes * ChannelCount;
                if (b >= HeaderChunk.TotalInterlacedBlockCount - 1 && c > 0) {
                    position += HeaderChunk.FinalBlockSizeWithPad * c -
                                HeaderChunk.BlockSizeBytes * (ChannelCount + ChannelCount - c);
                }
            } else {
                position = b * HeaderChunk.BlockSizeBytes;
            }

            Span<byte> block = new byte[curBlockSize];
            Handle.Read(HeaderChunk.DataOffsetAbsolute + position, block);

            switch (HeaderChunk.Codec) {
                case BrstmCodec.PCM8:
                    for (int sampleIdx = 0; sampleIdx < curBlockSamples; sampleIdx++) {
                        if (!IsBrwav) {
                            pcmSamples[c1, outputPos++] =
                                (short)(block[sampleIdx] * 0xFF);
                        } else {
                            pcmSamples[c1, outputPos++] =
                                (short)(block[sampleIdx * ChannelCount + c] * 0xFF);
                        }
                    }

                    break;
                case BrstmCodec.PCM16:
                    for (int sampleIdx = 0; sampleIdx < curBlockSamples; sampleIdx++) {
                        if (!IsBrwav) {
                            pcmSamples[c1, outputPos++] =
                                BinaryPrimitives.ReadInt16BigEndian(block[(sampleIdx * 2)..]);
                        } else {
                            pcmSamples[c1, outputPos++] =
                                BinaryPrimitives.ReadInt16BigEndian(block[(sampleIdx * 2 * ChannelCount + c * 2)..]);
                        }
                    }

                    break;
                case BrstmCodec.ADPCM4: {
                    int writtenSamples = 0;
                    if (IsBrwav) break;

                    byte ps = block[0];
                    int yn1 = AdpcmHsamples1[c, b],
                        yn2 = AdpcmHsamples2[c, b];

                    int cps = ps,
                        cyn1 = yn1,
                        cyn2 = yn2;
                    int dataIdx = 0;

                    for (int sampleIdx = 0; sampleIdx < curBlockSamples; sampleIdx++) {
                        if (sampleIdx % 14 == 0) {
                            cps = block[dataIdx++];
                        }

                        long outSample = (sampleIdx++ & 1) == 0 ? block[dataIdx] >> 4 : block[dataIdx++] & 0x0F;
                        long scale = 1 << (cps & 0x0F);
                        long cIdx = cps >> 4 << 1;

                        // ???????????
                        outSample = (0x400 + ((scale * outSample) << 11) + AdpcmCoeffs[c, Math.Clamp(cIdx, 0x0, 0xF)]
                            * cyn1 + AdpcmCoeffs[c, Math.Clamp(cIdx + 1, 0x0, 0xF)] * cyn2) >> 11;
                        cyn2 = cyn1;
                        cyn1 = (int)Math.Clamp(outSample, short.MinValue, short.MaxValue);

                        pcmSamples[c1, outputPos++] = (short)cyn1;
                        writtenSamples++;
                    }

                    if (b < HeaderChunk.TotalInterlacedBlockCount - 1) {
                        AdpcmHsamples1[c, b + 1] = pcmSamples[c1, HeaderChunk.SamplesPerBlock + writtenSamples - 1];
                        AdpcmHsamples2[c, b + 1] = pcmSamples[c1, HeaderChunk.SamplesPerBlock + writtenSamples - 2];
                    }

                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(HeaderChunk.Codec));
            }
        }
    }
}