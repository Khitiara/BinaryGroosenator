using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using NAudio.Wave;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Brstm.IO
{
    public class BrstmWriter
    {
        public string OriginalFilePath;
        public string OutputDirectory;

        public BrstmWriter(string originalFilePath, string outputDirectory) {
            OriginalFilePath = originalFilePath;
            OutputDirectory = outputDirectory;
        }

        public void WriteTracks() {
            using MemoryMappedFile file = MemoryMappedFile.CreateFromFile(OriginalFilePath);
            using MemoryMappedViewAccessor accessor = file.CreateViewAccessor();
            BrstmReader reader = new BrstmReader(accessor);
            reader.Read();

            List<BrstmTrackMetadata> metadata = new();

            for (int index = 0; index < reader.Tracks.Length; index++) {
                BrstmInterleavingWaveProvider track = reader.WriteTrack(index);
                string outPath = Path.Combine(OutputDirectory,
                    $"{Path.GetFileNameWithoutExtension(OriginalFilePath)}.{index}.brstm.wav");
                WaveFileWriter.CreateWaveFile(outPath, track);
                metadata.Add(new BrstmTrackMetadata {
                    TrackId = index,
                    LoopStart = track.LoopStartSample,
                    WavFileName = Path.GetRelativePath(OutputDirectory, outPath)
                });
            }

            using StreamWriter metaDataStream = File.CreateText(Path.Combine(OutputDirectory,
                $"{Path.GetFileNameWithoutExtension(OriginalFilePath)}.brstm.json"));
            using JsonTextWriter writer = new JsonTextWriter(metaDataStream);
            JsonSerializer serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                ContractResolver = new DefaultContractResolver {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                }
            });
            serializer.Serialize(writer, metadata);
        }
    }
}