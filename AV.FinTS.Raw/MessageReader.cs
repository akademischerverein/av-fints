using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public class MessageReader
    {
        private static NumberFormatInfo CommaDecimal;
        static MessageReader() {
            CommaDecimal = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();
            CommaDecimal.NumberDecimalSeparator = ",";
        }

        private const int Group_Delimiter = 0x1;
        private const int Element_Delimiter = 0x2;
        private const int Segment_Delimiter = 0x3;
        private readonly MemoryStream _stream;
        private int _group = 0;

        public bool GroupEnded { get; private set; } = false;

        public bool SegmentEnded { get; private set; } = false;

        public bool FirstSegmentElement { get; private set; } = true;

        internal long Position => _stream.Position;

        internal long Length => _stream.Length;

        public MessageReader(byte[] msg)
        {
            _stream = new MemoryStream(msg);
        }

        public SegmentId ReadSegmentHeader()
        {
            if (!FirstSegmentElement)
            {
                throw new InvalidOperationException("Not first element in segment");
            }
            EnterGroup();
            var name = Read();
            var number = ReadInt();
            var version = ReadInt();
            var reference = ReadInt();
            LeaveGroup();

            if (name == null || version == null || number == null)
            {
                throw new InvalidOperationException("invalid header detected");
            }

            return new SegmentId
            {
                Name = name,
                Version = (int)version,
                Number = (int)number,
                SegmentReference = reference
            };
        }

        private byte ReadByte()
        {
            if (SegmentEnded)
            {
                throw new InvalidOperationException("can't read beyond the end of a segment");
            }

            var b = _stream.ReadByte();
            GroupEnded = false;
            if (b == 0x3f)
            {
                return (byte)_stream.ReadByte();
            } else if (b == 0x3a)
            {
                FirstSegmentElement = false;
                return Group_Delimiter;
            } else if (b == 0x2b)
            {
                FirstSegmentElement = false;
                GroupEnded = true;
                return Element_Delimiter;
            } else if (b == 0x27)
            {
                FirstSegmentElement = false;
                GroupEnded = true;
                SegmentEnded = true;
                return Segment_Delimiter;
            }
            return (byte)b;
        }

        private byte ReadInternal(out string? result)
        {
            var bytes = new MemoryStream();
            var b = ReadByte();
            while (b != Group_Delimiter && b != Element_Delimiter && b != Segment_Delimiter)
            {
                bytes.WriteByte(b);
                b = ReadByte();
            }

            if (bytes.Position == 0)
            {
                result = null;
            } else
            {
                result = Encoding.Latin1.GetString(bytes.ToArray());
            }
            return b;
        }

        private string? ReadRootElement()
        {
            if (SegmentEnded)
            {
                return null;
            }
            var ret = ReadInternal(out var ele);
            if (ret == Group_Delimiter)
            {
                throw new Exception("Element is not a root element");
            }
            return ele;
        }

        private string? ReadRootGroupElement()
        {
            if (SegmentEnded || GroupEnded)
            {
                return null;
            }
            var ret = ReadInternal(out var ele);
            return ele;
        }

        private string? ReadNestedGroupElement()
        {
            if (SegmentEnded || GroupEnded)
            {
                return null;
            }
            var ret = ReadInternal(out var ele);
            return ele;
        }

        public void EnterGroup()
        {
            _group++;
            GroupEnded = SegmentEnded;
        }

        public void LeaveGroup()
        {
            if (_group == 0)
            {
                throw new Exception("Not in group");
            }
            if (!GroupEnded && _group == 1)
            {
                throw new Exception("Group has not ended");
            }
            if (_group == 1)
            {
                GroupEnded = false;
            }
            _group--;
        }

        public string? Read()
        {
            if (_group == 0)
            {
                return ReadRootElement();
            } else if (_group == 1)
            {
                return ReadRootGroupElement();
            } else
            {
                return ReadNestedGroupElement();
            }
        }

        public void SkipCurrentSegment()
        {
            int b;
            bool nextElement = true;
            bool prefixStart = false;
            while((b = ReadByte()) != Segment_Delimiter)
            {
                if (b == Element_Delimiter ||  b == Group_Delimiter)
                {
                    nextElement = true;
                } else if (nextElement)
                {
                    nextElement = false;
                    if (b == '@')
                    {
                        prefixStart = true;
                    }
                } else if (prefixStart)
                {
                    if ((b < 0x30 || b > 0x39) && b != 0x40)
                    {
                        prefixStart = false;
                    } else if (b == 0x40)
                    {
                        throw new Exception("binary fields not supported for skipping");
                    }
                }
            }

            _group = 0;
        }

        public int? ReadInt()
        {
            var s = Read();
            if (s == null)
            {
                return null;
            }
            return int.Parse(s);
        }

        public DateOnly? ReadDate()
        {
            var date = Read();
            if (date == null)
            {
                return null;
            }
            return DateOnly.ParseExact(date, "yyyyMMdd");
        }

        public TimeOnly? ReadTime()
        {
            var date = Read();
            if (date == null)
            {
                return null;
            }
            return TimeOnly.ParseExact(date, "HHmmss");
        }

        public bool? ReadBool()
        {
            var b = Read();
            if (b == null)
            {
                return null;
            }
            return b == "J";
        }

        public decimal? ReadDecimal()
        {
            var d = Read();
            if (d == null)
            {
                return null;
            }
            return decimal.Parse(d, CommaDecimal);
        }

        public byte[] ReadBytes()
        {
            if (SegmentEnded)
            {
                return Array.Empty<byte>();
            }
            var prefixStart = ReadByte();
            if (prefixStart == Segment_Delimiter || prefixStart == Group_Delimiter || prefixStart == Element_Delimiter)
            {
                return Array.Empty<byte>();
            }
            if (prefixStart != 0x40)
            {
                throw new InvalidOperationException("Invalid binary field");
            }
            int b;
            var asciiLength = string.Empty;
            while ((b = ReadByte()) != 0x40) {
                if (b < 0x30 || b > 0x39)
                {
                    throw new InvalidOperationException("Invalid binary field");
                }
                asciiLength += (char)b;
            }
            var bytes = new byte[int.Parse(asciiLength)];
            _stream.ReadExactly(bytes);

            b = ReadByte();
            if (b != Segment_Delimiter && b != Group_Delimiter && b != Element_Delimiter)
            {
                throw new InvalidOperationException("Invalid stream state");
            }
            return bytes;
        }

        public void StartNextSegment()
        {
            if (_stream.Position == 0) return;
            if (!SegmentEnded)
            {
                throw new InvalidOperationException("read position is not at an end of a segment");
            }
            if (_group != 0)
            {
                throw new InvalidOperationException("still in group");
            }
            SegmentEnded = false;
            FirstSegmentElement = true;
            _group = 0;
        }

        public bool Available()
        {
            return _stream.Position < _stream.Length;
        }
    }
}
