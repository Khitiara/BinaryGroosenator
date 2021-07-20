using System;
using System.IO;
using Brsar.IO;
using Brsar.Sound.Data;
using Brstm;
using Brstm.IO;
using NxCore;

namespace Brsar
{
    public class LazyStrmData : LazySoundData
    {
        internal LazyStrmData(BrsarReader reader, SoundDataEntry entry) : base(reader, entry) { }

        public void WriteOut(string directory) {
            BrstmWriter writer = new(Path.Combine(Reader.WorkingDir,
                CollectionExternalFilePath ?? throw new InvalidOperationException()), directory);
            writer.WriteTracks();
        }
    }
}