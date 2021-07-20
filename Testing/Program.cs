using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using Brsar;

namespace Testing
{
    class Program
    {
        static void Main() {
            string workingDir = @"D:\Shared\roms\sshd\out\merge\romfs\Sound";
            using MemoryMappedFile file =
                MemoryMappedFile.CreateFromFile(Path.Combine(workingDir, "WZSound.brsar"));
            using BrsarReader reader = new(workingDir, file);
            reader.Read();

            string outWorkDir = @"D:\Shared\roms\sshd\out\merge\romfs\Sound\Processed";
            Directory.CreateDirectory(outWorkDir);

            for (uint i = 0; i < reader.SoundDataCount ; i++) {
                reader.MarshalSoundData(i, out LazySoundData data);
                if (data is not LazyStrmData streamData) continue;
                Console.WriteLine(streamData.Filename);
                string outDir = Path.Combine(outWorkDir, streamData.Filename);
                Directory.CreateDirectory(outDir);
                streamData.WriteOut(outDir);
            }
        }
    }
}