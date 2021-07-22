using System;
using System.IO;
using Arc;
using Brsar;
using Dialogue.Msbt;
using Khiti.Compression.NZLSS;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;

namespace Testing
{
    internal static class Program
    {
        internal static void Main() {
            // SoundTest();
            // ArcTest();
            MsbtTest();
        }

        private static void MsbtTest() {
            string arcPath = @"D:\Shared\roms\sshd\out\merge\romfs\US\Object\en_US\2-Forest.arc";
            using LocalStorage storage = new(arcPath, FileAccess.Read);
            using ArcReader reader = new(storage);
            reader.Read();
            Console.WriteLine("\n");
            foreach ((uint id, ArcNode node) in reader.Nodes) {
                Console.WriteLine(
                    $"{{Id: {id:X8}, Name: {reader.Paths[id]}, Offset: {node.DataOffset:X8}, Size: {node.Size:X8}}}");
            }

            Console.WriteLine("\n");
            string pathInArc = "/2-Forest/200-Forest.msbt";
            using IFileSystem fs = reader.Open();
            fs.OpenFile(out IFile file, (U8Span)pathInArc, OpenMode.Read).ThrowIfFailure();
            using (file)
            using (MsbtReader msbtReader = new(file.AsStorage())) {
                msbtReader.Read();
                foreach ((string label, string text) in msbtReader.AllText) {
                    Console.WriteLine($"{label}:\t{text}");
                }
            }
        }

        private static void ArcTest() {
            string dir = @"D:\Shared\roms\sshd\out\merge\romfs\Stage\F301_3\NX";
            string inFile = @"F301_3_stg_l0.arc.LZ";
            string inPath = Path.Combine(dir, inFile);
            string outFile = Path.GetFileNameWithoutExtension(inFile);
            string outPath = Path.Combine(dir, outFile);
            if (!File.Exists(outPath)) {
                using FileStream inStream = File.OpenRead(inPath);
                using FileStream outStream = File.OpenWrite(outPath);
                Console.WriteLine(LZSS.Decompress(inStream, inStream.Length, outStream));
            }

            using LocalStorage storage = new(outPath, FileAccess.Read);
            using ArcReader reader = new(storage);
            reader.Read();
            Console.WriteLine("\n");
            foreach ((uint id, ArcNode node) in reader.Nodes) {
                Console.WriteLine(
                    $"{{Id: {id:X8}, Name: {reader.Paths[id]}, Offset: {node.DataOffset:X8}, Size: {node.Size:X8}}}");
            }
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