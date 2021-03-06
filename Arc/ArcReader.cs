using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Util;
using NxCore;

namespace Arc
{
    public class ArcReader : BinaryRevolutionReader
    {
        public const    uint                      ArcHeaderMagic = 0x55AA382D;
        public readonly Dictionary<uint, string>  Names;
        public readonly Dictionary<uint, ArcNode> Nodes;
        public readonly Dictionary<uint, uint>    Parents;
        public readonly Dictionary<string, uint>  PathLookup;
        public readonly Dictionary<uint, string>  Paths;

        public ArcHeader Header;
        public uint      StringTableOffset;

        public ArcReader(IStorage handle) {
            Handle = handle;
            Parents = new Dictionary<uint, uint>();
            Paths = new Dictionary<uint, string>();
            Names = new Dictionary<uint, string>();
            PathLookup = new Dictionary<string, uint>();
            Nodes = new Dictionary<uint, ArcNode>();
        }

        public bool Initialized { get; private set; }

        public override IStorage Handle { get; }

        public override void Read() {
            DoMarshal(0, out Header);
            if (Header.Magic != ArcHeaderMagic)
                throw new InvalidDataException("Corrupt ARC header");

            DoMarshal(Header.RootNodeOffset, out ArcNode rootNode);
            Nodes[0] = rootNode;
            Parents[0] = 0;
            Paths[0] = Names[0] = "/";
            PathLookup["/"] = 0;
            if (rootNode.Type != ArcNodeType.Directory)
                throw new InvalidDataException("Root node must be a directory!");
            StringTableOffset = Header.RootNodeOffset + rootNode.Size * 0xC;

            Stack<uint> lastNodes = new();
            lastNodes.Push(rootNode.Size);
            uint currentDirectory = 0, offset = Header.RootNodeOffset;
            for (uint i = 1; i < rootNode.Size; i++) {
                if (i == lastNodes.Peek()) {
                    lastNodes.Pop();
                    currentDirectory = Parents[currentDirectory];
                }

                DoMarshal(offset + 0xC * i, out ArcNode node);
                uint stringTableOffset = StringTableOffset + node.NameOffset;
                Console.WriteLine($"Name {i} at {stringTableOffset:X8}");
                string name = Handle.ReadNullTerm(stringTableOffset, 255);
                string path = PathTools.Combine(Paths[currentDirectory], name);
                Nodes[i] = node;
                Names[i] = name;
                Paths[i] = path;
                PathLookup[path] = i;
                Parents[i] = currentDirectory;
                if (node.Type == ArcNodeType.Directory) {
                    currentDirectory = i;
                    lastNodes.Push(node.Size);
                }
            }

            Initialized = true;
        }

        public IFileSystem Open() {
            if (!Initialized) Read();
            return new ArcFileSystem(this);
        }

        private class ArcFileSystem : IFileSystem
        {
            private readonly ArcReader _reader;

            public ArcFileSystem(ArcReader reader) {
                _reader = reader;
            }

            protected override Result DoCreateDirectory(U8Span path) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoCreateFile(U8Span path, long size, CreateFileOptions options) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoDeleteDirectory(U8Span path) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoDeleteDirectoryRecursively(U8Span path) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoCleanDirectoryRecursively(U8Span path) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoDeleteFile(U8Span path) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoRenameDirectory(U8Span oldPath, U8Span newPath) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();

            protected override Result DoRenameFile(U8Span oldPath, U8Span newPath) =>
                ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();


            protected override Result DoGetEntryType(out DirectoryEntryType entryType, U8Span path) {
                UnsafeHelpers.SkipParamInit(out entryType);

                string pathString = path.ToString();
                if (!_reader.PathLookup.ContainsKey(pathString)) return ResultFs.PathNotFound.Log();
                uint id = _reader.PathLookup[pathString];
                entryType = _reader.Nodes[id].Type switch {
                    ArcNodeType.File => DirectoryEntryType.File,
                    ArcNodeType.Directory => DirectoryEntryType.Directory,
                    _ => throw new ArgumentOutOfRangeException()
                };
                return Result.Success;
            }

            protected override Result DoOpenFile(out IFile file, U8Span path, OpenMode mode) {
                UnsafeHelpers.SkipParamInit(out file);
                if (mode != OpenMode.Read)
                    return ResultFs.UnsupportedWriteForReadOnlyFileSystem.Log();
                string pathString = path.ToString();
                if (!_reader.PathLookup.ContainsKey(pathString))
                    return ResultFs.PathNotFound.Log();
                uint id = _reader.PathLookup[pathString];
                ArcNode node = _reader.Nodes[id];
                if (node.Type != ArcNodeType.File)
                    return ResultFs.PathNotFound.Log();
                file = _reader.Handle.Slice(node.DataOffset, node.Size).AsFile(mode);
                return Result.Success;
            }

            protected override Result DoOpenDirectory(out IDirectory directory, U8Span path, OpenDirectoryMode mode) {
                UnsafeHelpers.SkipParamInit(out directory);
                string pathString = path.ToString();
                if (!_reader.PathLookup.ContainsKey(pathString))
                    return ResultFs.PathNotFound.Log();
                uint id = _reader.PathLookup[pathString];
                ArcNode node = _reader.Nodes[id];
                if (node.Type != ArcNodeType.Directory)
                    return ResultFs.PathNotFound.Log();
                directory = new ArcFsDirectory(_reader, id);
                return Result.Success;
            }

            protected override Result DoCommit() {
                return Result.Success;
            }

            private class ArcFsDirectory : IDirectory
            {
                private readonly uint      _id;
                private readonly ArcReader _reader;
                private          uint[]    _childIds;
                private          bool      _initialized;

                public ArcFsDirectory(ArcReader reader, uint id) {
                    _reader = reader;
                    _id = id;
                    _initialized = false;
                }

                private void Init() {
                    _childIds = _reader.Parents.Where(kvp => kvp.Value == _id).Select(kvp => kvp.Key).ToArray();
                }

                protected override Result DoRead(out long entriesRead, Span<DirectoryEntry> entryBuffer) {
                    if (!_initialized) Init();
                    entriesRead = Math.Max(entryBuffer.Length, _childIds.Length);
                    for (int i = 0; i < entriesRead; i++) {
                        uint id = _childIds[i];
                        StringUtils.Copy(entryBuffer[i].Name, StringUtils.StringToUtf8(_reader.Names[id]));
                        entryBuffer[i].Attributes = _reader.Nodes[id].Type == ArcNodeType.Directory
                            ? NxFileAttributes.Directory
                            : NxFileAttributes.None;
                        entryBuffer[i].Type = _reader.Nodes[id].Type switch {
                            ArcNodeType.File => DirectoryEntryType.File,
                            ArcNodeType.Directory => DirectoryEntryType.Directory,
                            _ => throw new ArgumentOutOfRangeException()
                        };
                        entryBuffer[i].Size = _reader.Nodes[id].Size;
                    }

                    return Result.Success;
                }

                protected override Result DoGetEntryCount(out long entryCount) {
                    if (!_initialized) Init();
                    entryCount = _childIds.Length;
                    return Result.Success;
                }
            }
        }
    }
}