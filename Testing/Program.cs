using System;
using System.IO;
using Arc;
using Brsar;
using LibHac.Fs;
using LibHac.FsSystem;

namespace Testing
{
    internal static class Program
    {
        internal static void Main() {
            // SoundTest();
            ArcTest.Decompress();
        }

        private static void SoundTest() {
            string workingDir = @"D:\Shared\roms\sshd\out\merge\romfs\Sound";
            using LocalFile file = new(workingDir, OpenMode.Read);
            using BrsarReader reader = new(new LocalFileSystem(), workingDir, file);
            reader.Read();

            string outWorkDir = @"D:\Shared\roms\sshd\out\merge\romfs\Sound\Processed";
            Directory.CreateDirectory(outWorkDir);

            for (uint i = 0; i < reader.SoundDataCount; i++) {
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