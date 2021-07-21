using System;
using Brsar.Sound.Data;
using Brstm.IO;
using LibHac.FsSystem;

namespace Brsar
{
    public class LazyStrmData : LazySoundData
    {
        internal LazyStrmData(BrsarReader reader, SoundDataEntry entry) : base(reader, entry) { }

        public void WriteOut(string directory) {
            BrstmWriter writer = new(Reader.WorkingFs, PathTools.Combine(Reader.WorkingDir,
                CollectionExternalFilePath ?? throw new InvalidOperationException()), directory);
            writer.WriteTracks();
        }
    }
}