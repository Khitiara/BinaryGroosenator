using System;
using System.Runtime.InteropServices;
using NAudio.Wave;

namespace Brstm
{
    public class BrstmInterleavingWaveProvider : IWaveProvider
    {
        private readonly short[,] _pcmSampleData;
        private          int      _idx;

        public BrstmInterleavingWaveProvider(HeadChunk header, TrackDescription track, short[,] pcmSampleData) {
            _pcmSampleData = pcmSampleData;
            Codec = header.Codec;
            SampleRate = header.SampleRate;
            IsLooping = header.LoopFlag;
            LoopStartSample = IsLooping ? header.LoopStartSampleIdx : null;
            Volume = track.Volume;
            Channels = _pcmSampleData.GetLength(0);
            Samples = _pcmSampleData.GetLength(1);
        }

        public int Samples { get; }
        public byte Volume { get; }
        public BrstmCodec Codec { get; }
        public ushort SampleRate { get; }
        public bool IsLooping { get; }
        public uint? LoopStartSample { get; }
        public int Channels { get; }

        public WaveFormat WaveFormat => new(SampleRate, 16, Channels);

        public int Read(byte[] buffer, int offset, int count) {
            Span<byte> span = buffer.AsSpan(offset, count);
            int i;
            for (i = 0; i < span.Length && _idx < _pcmSampleData.Length; i += 2, _idx++) {
                int sample = _idx / Channels;
                int channel = _idx % Channels;
                MemoryMarshal.Write(span.Slice(i, 2), ref _pcmSampleData[channel, sample]);
            }

            return i;
        }
    }
}