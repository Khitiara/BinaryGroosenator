using System.IO.MemoryMappedFiles;
using NxCore;

namespace Brsar.IO
{
    public sealed class ExtFileStorage : IDataStorage
    {
        private MemoryMappedFile _handle;

        public ExtFileStorage(MemoryMappedFile handle) {
            _handle = handle;
        }

        public void Dispose() {
            _handle.Dispose();
        }

        public MemoryMappedViewAccessor Open() => _handle.CreateViewAccessor();
    }
}