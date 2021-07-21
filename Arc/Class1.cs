using System;
using System.IO;
using Khiti.Compression.NZLSS;

namespace Arc
{
    public class ArcTest
    {
        public static void Decompress() {
            string dir = @"D:\Shared\roms\sshd\out\merge\romfs\Stage\F301\NX";
            string inFile = @"F301_stg_l0.arc.LZ";
            string inPath = Path.Combine(dir, inFile);
            string outFile = Path.GetFileNameWithoutExtension(inFile);
            string outPath = Path.Combine(dir, outFile);
            using FileStream inStream = File.OpenRead(inPath);
            using FileStream outStream = File.OpenWrite(outPath);
            Console.WriteLine(LZSS.Decompress(inStream, inStream.Length, outStream));
        }
    }
}