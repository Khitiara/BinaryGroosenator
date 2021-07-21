using System;
using System.Collections.Generic;
using System.IO;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brstm.IO
{
    public class BrstmWriter : IDisposable
    {
        private readonly IFileSystem _fileSystem;

        private readonly JsonSerializer _serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        });

        public IFile  OriginalFile;
        public string OriginalFilePath;
        public string OutputDirectory;

        public BrstmWriter(IFileSystem fileSystem, string originalFilePath, string outputDirectory) {
            _fileSystem = fileSystem;
            OriginalFilePath = originalFilePath;
            fileSystem.OpenFile(out OriginalFile, new U8Span(originalFilePath), OpenMode.Read).ThrowIfFailure();
            OutputDirectory = outputDirectory;
        }

        public void Dispose() {
            OriginalFile.Dispose();
        }

        public void WriteTracks() {
            BrstmReader reader = new(OriginalFile.AsStorage());
            reader.Read();

            List<BrstmTrackMetadata> metadata = new();

            for (int index = 0; index < reader.Tracks.Length; index++) {
                BrstmInterleavingWaveProvider track = reader.WriteTrack(index);
                string outPath = Path.Combine(OutputDirectory,
                    $"{Path.GetFileNameWithoutExtension(OriginalFilePath)}.{index}.brstm.wav");
                _fileSystem.OpenFile(out IFile wavFile, new U8Span(outPath), OpenMode.Write).ThrowIfFailure();
                using (wavFile)
                    WaveFileWriter.WriteWavFileToStream(wavFile.AsStream(), track);
                metadata.Add(new BrstmTrackMetadata {
                    TrackId = index,
                    LoopStart = track.LoopStartSample,
                    WavFileName = Path.GetRelativePath(OutputDirectory, outPath)
                });
            }

            string metaPath = PathTools.Combine(OutputDirectory,
                $"{Path.GetFileNameWithoutExtension(OriginalFilePath)}.brstm.json");
            _fileSystem.OpenFile(out IFile file, new U8Span(metaPath), OpenMode.Write).ThrowIfFailure();
            using (file)
            using (Stream stream = file.AsStream())
            using (StreamWriter metaDataStream = new(stream))
            using (JsonTextWriter writer = new(metaDataStream))
                _serializer.Serialize(writer, metadata);
        }
    }
}