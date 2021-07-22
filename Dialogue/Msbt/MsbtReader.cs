using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using LibHac.Fs;
using LibHac.Util;
using NxCore;

namespace Dialogue.Msbt
{
    public class MsbtReader : BinaryRevolutionReader
    {
        public const ulong                      MsbtMagic = 0x4D7367537464426E;
        public const uint                       Lbl1Magic = 0x4C424C31;
        public const uint                       Atr1Magic = 0x41545231;
        public const uint                       Txt2Magic = 0x54585432;
        public       Dictionary<string, string> AllText;
        public       Atr1Header                 Atr1;
        public       uint                       Atr1Offset;

        public MsbtHeader Header;

        public Dictionary<uint, Dictionary<string, uint>> LabelLookup;
        public Lbl1Header                                 Lbl1;

        public uint                                         Lbl1Offset = 0x20; // always the same
        public Dictionary<uint, Dictionary<string, string>> Text;
        public Dictionary<uint, string>                     TextByIndex;
        public Txt2Header                                   Txt2;
        public uint                                         Txt2Offset;

        public MsbtReader(IStorage handle) {
            Handle = handle;
            LabelLookup = new Dictionary<uint, Dictionary<string, uint>>();
            Text = new Dictionary<uint, Dictionary<string, string>>();
            TextByIndex = new Dictionary<uint, string>();
            AllText = new Dictionary<string, string>();
        }

        public override IStorage Handle { get; }

        public override void Read() {
            DoMarshal(0, out Header);
            if (Header.Magic != MsbtMagic)
                throw new InvalidDataException("Corrupt MSBT header");

            DoMarshal(0x20, out Lbl1);
            if (Lbl1.Magic != Lbl1Magic)
                throw new InvalidDataException("Corrupt LBL1 header");

            for (uint i = 0; i < Lbl1.EntryCount; i++) {
                DoMarshal((uint)(Lbl1Offset + 0x14 + i * Unsafe.SizeOf<Lbl1Entry>()), out Lbl1Entry entry);
                Dictionary<string, uint> dictionary = new();
                LabelLookup[i] = dictionary;
                uint runningOffset = Lbl1Offset + 0x10 + entry.Offset;
                for (int j = 0; j < entry.PairCount; j++) {
                    byte lblLen = Handle.ReadByte(runningOffset++);
                    Span<byte> span = new byte[lblLen];
                    Handle.Read(runningOffset, span).ThrowIfFailure();
                    string lbl = StringUtils.Utf8ToString(span);
                    runningOffset += lblLen;
                    dictionary[lbl] = Handle.ReadUInt32BigEndian(runningOffset);
                    runningOffset += 4;
                }
            }

            Atr1Offset = Lbl1Offset + 0x10 + Utils.RoundUp16(Lbl1.SectionSize);
            DoMarshal(Atr1Offset, out Atr1);
            if (Atr1.Magic != Atr1Magic)
                throw new InvalidDataException("Corrupt ATR1 header");

            // TODO: Figure out ATR1 for lozsshd

            Txt2Offset = Atr1Offset + 0x10 + Utils.RoundUp16(Atr1.SectionSize);
            DoMarshal(Txt2Offset, out Txt2);
            if (Txt2.Magic != Txt2Magic)
                throw new InvalidDataException("Corrupt TXT2 header");

            uint[] offsets = new uint[Txt2.EntryCount + 1];
            for (int i = 0; i < Txt2.EntryCount; i++) {
                offsets[i] = Txt2Offset + 0x10 + Handle.ReadUInt32BigEndian(Txt2Offset + 0x14 + i * 0x4);
            }

            offsets[Txt2.EntryCount] = Txt2Offset + 0x10 + Txt2.SectionSize;

            for (uint i = 0; i < Txt2.EntryCount; i++) {
                uint len = offsets[i + 1] - offsets[i];
                Span<byte> span = new byte[len];
                Handle.Read(offsets[i], span);
                TextByIndex[i] = Encoding.BigEndianUnicode.GetString(span);
            }

            foreach ((uint entry, var dict) in LabelLookup) {
                Text[entry] = new Dictionary<string, string>();
                foreach ((var lbl, uint txtIdx) in dict) {
                    Text[entry][lbl] = TextByIndex[txtIdx];
                    if (AllText.ContainsKey(lbl))
                        throw new IOException("HI");
                    AllText[lbl] = TextByIndex[txtIdx];
                }
            }
        }
    }
}