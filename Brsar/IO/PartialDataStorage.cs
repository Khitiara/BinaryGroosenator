using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using NxCore;

namespace Brsar.IO
{
    public abstract class PartialDataStorage : IDataStorage
    {
        protected readonly MemoryMappedViewAccessor Header;
        protected readonly MemoryMappedViewAccessor Data;

        protected PartialDataStorage(MemoryMappedViewAccessor header, MemoryMappedViewAccessor data) {
            Header = header;
            Data = data;
        }

        public static PartialDataStorage PartialLoad(MemoryMappedViewAccessor header, MemoryMappedViewAccessor data) {
            throw new NotImplementedException();
        }

        public void Dispose() {
            Header.Dispose();
            Data.Dispose();
        }
    }
}