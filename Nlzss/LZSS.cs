using System;
using System.Buffers.Binary;
using System.IO;

namespace Khiti.Compression.NZLSS
{
    public static class LZSS
    {
        public static long Decompress(Stream instream, long inLength, Stream outstream) {
            long readBytes = 0;
            byte type = (byte)instream.ReadByte();
            if (type != 0x11) {
                throw new InvalidDataException("The provided stream is not a valid LZ-0x11 "
                                               + "compressed stream (invalid type 0x" + type.ToString("X") + ")");
            }

            Span<byte> sizeBytes = new byte[4];
            instream.Read(sizeBytes.Slice(0, 3));
            int decompressedSize = BinaryPrimitives.ReadInt32LittleEndian(sizeBytes);
            readBytes += 4;
            if (decompressedSize == 0) {
                instream.Read(sizeBytes);
                decompressedSize = BinaryPrimitives.ReadInt32LittleEndian(sizeBytes);
                readBytes += 4;
            }

            // the maximum 'DISP-1' is still 0xFFF.
            int bufferLength = 0x1000;
            byte[] buffer = new byte[bufferLength];
            int bufferOffset = 0;
            int currentOutSize = 0;
            int flags = 0, mask = 1;
            while (currentOutSize < decompressedSize) {
                // (throws when requested new flags byte is not available)

                // the current mask is the mask used in the previous run. So if it masks the
                // last flag bit, get a new flags byte.
                if (mask == 1) {
                    if (readBytes >= inLength)
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    flags = instream.ReadByte();
                    readBytes++;
                    if (flags < 0)
                        throw new StreamTooShortException();
                    mask = 0x80;
                } else {
                    mask >>= 1;
                }

                // bit = 1 <=> compressed.
                if ((flags & mask) > 0) {
                    // (throws when not enough bytes are available)

                    // read the first byte first, which also signals the size of the compressed block
                    if (readBytes >= inLength)
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    int byte1 = instream.ReadByte();
                    readBytes++;
                    if (byte1 < 0)
                        throw new StreamTooShortException();
                    int length = byte1 >> 4;
                    int disp;
                    if (length == 0) {
                        // case 0:
                        // data = AB CD EF (with A=0)
                        // LEN = ABC + 0x11 == BC + 0x11
                        // DISP = DEF + 1
                        // we need two more bytes available
                        if (readBytes + 1 >= inLength)
                            throw new NotEnoughDataException(currentOutSize, decompressedSize);
                        int byte2 = instream.ReadByte();
                        readBytes++;
                        int byte3 = instream.ReadByte();
                        readBytes++;
                        if (byte3 < 0)
                            throw new StreamTooShortException();
                        length = (((byte1 & 0x0F) << 4) | (byte2 >> 4)) + 0x11;
                        disp = (((byte2 & 0x0F) << 8) | byte3) + 0x1;
                    } else if (length == 1) {
                        // case 1:
                        // data = AB CD EF GH (with A=1)
                        // LEN = BCDE + 0x111
                        // DISP = FGH + 1
                        // we need three more bytes available
                        if (readBytes + 2 >= inLength)
                            throw new NotEnoughDataException(currentOutSize, decompressedSize);
                        int byte2 = instream.ReadByte();
                        readBytes++;
                        int byte3 = instream.ReadByte();
                        readBytes++;
                        int byte4 = instream.ReadByte();
                        readBytes++;
                        if (byte4 < 0)
                            throw new StreamTooShortException();
                        length = (((byte1 & 0x0F) << 12) | (byte2 << 4) | (byte3 >> 4)) + 0x111;
                        disp = (((byte3 & 0x0F) << 8) | byte4) + 0x1;
                    } else {
                        // case other:
                        // data = AB CD
                        // LEN = A + 1
                        // DISP = BCD + 1
                        // we need only one more byte available
                        if (readBytes >= inLength)
                            throw new NotEnoughDataException(currentOutSize, decompressedSize);
                        int byte2 = instream.ReadByte();
                        readBytes++;
                        if (byte2 < 0)
                            throw new StreamTooShortException();
                        length = ((byte1 & 0xF0) >> 4) + 0x1;
                        disp = (((byte1 & 0x0F) << 8) | byte2) + 0x1;
                    }

                    if (disp > currentOutSize)
                        throw new InvalidDataException("Cannot go back more than already written. "
                                                       + "DISP = " + disp + ", #written bytes = 0x" +
                                                       currentOutSize.ToString("X")
                                                       + " before 0x" + instream.Position.ToString("X") +
                                                       " with indicator 0x"
                                                       + (byte1 >> 4).ToString("X"));

                    int bufIdx = bufferOffset + bufferLength - disp;
                    for (int i = 0; i < length; i++) {
                        byte next = buffer[bufIdx % bufferLength];
                        bufIdx++;
                        outstream.WriteByte(next);
                        buffer[bufferOffset] = next;
                        bufferOffset = (bufferOffset + 1) % bufferLength;
                    }

                    currentOutSize += length;
                } else {
                    if (readBytes >= inLength)
                        throw new NotEnoughDataException(currentOutSize, decompressedSize);
                    int next = instream.ReadByte();
                    readBytes++;
                    if (next < 0)
                        throw new StreamTooShortException();
                    outstream.WriteByte((byte)next);
                    currentOutSize++;
                    buffer[bufferOffset] = (byte)next;
                    bufferOffset = (bufferOffset + 1) % bufferLength;
                }
            }

            if (readBytes < inLength) {
                // the input may be 4-byte aligned.
                //if ((readBytes ^ (readBytes & 3)) + 4 < inLength)
                //throw new TooMuchInputException(readBytes, inLength);
            }

            return decompressedSize;
        }
    }
}