using Brsar.Sound.Data;

namespace Brsar
{
    public class LazyWavData : LazySoundData
    {
        internal LazyWavData(BrsarReader reader, SoundDataEntry entry) : base(reader, entry) { }
    }
}