using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public class MessageWriter
    {
        private const int NO_LENGTH = -1;
        private readonly MemoryStream _stream = new();
        private readonly MemoryStream _rootEmptyStream = new();
        private int _groupDepth = 0;
        private bool _prevElement = false;
        private bool _firstInGroup = true;
        private bool _groupEmpty = true;

        public byte[] ToBytes()
        {
            return _stream.ToArray();
        }

        public void EnterGroup()
        {
            _groupDepth++;
            if (_groupDepth == 1)
            {
                _firstInGroup = true;
                _groupEmpty = true;
            }
        }

        public void CloseGroup()
        {
            if (_groupDepth == 0)
            {
                throw new InvalidOperationException("not in a group");
            }
            _groupDepth--;
        }

        public void EndSegment()
        {
            _prevElement = false;
            _groupDepth = 0;
            _stream.WriteByte((byte)'\'');
            _rootEmptyStream.SetLength(0);
        }

        public void WriteSegmentHead(SegmentId segmentId)
        {
            if (segmentId.Number == null)
            {
                throw new InvalidOperationException("Number of segment not set");
            }

            EnterGroup();
            Write(segmentId.Name, FieldType.ALPHA_NUMERIC, max_length: 6);
            Write((int)segmentId.Number, FieldType.NUMERIC, max_length: 3);
            Write(segmentId.Version, FieldType.NUMERIC, max_length: 3);
            CloseGroup();
        }

        public void Write(IElementStructure? element)
        {
            EnterGroup();
            if (element == null)
            {
                WriteEmpty();
            } else
            {
                element.Write(this);
            }
            CloseGroup();
        }

        public void Write(ISegment segment)
        {
            WriteSegmentHead(segment.Head);
            segment.Write(this);
            EndSegment();
        }

        private void CheckValidBytes(byte[] bytes, bool crLfAllowed=true)
        {
            var idx = 0;
            foreach(var b in bytes)
            {
                if(b > 0x1f && b < 0x7e && b != 0x60 && b != 0x7c)
                {
                    idx++;
                    continue;
                } else if (b == 0xa7 || b == 0xc4 || b == 0xd6 || b == 0xdc || b == 0xdf || b == 0xe4 || b == 0xf6 || b == 0xfc)
                {
                    idx++;
                    continue;
                } else if (crLfAllowed && (b == 0xa || b == 0xd))
                {
                    idx++;
                    continue;
                }

                throw new ArgumentOutOfRangeException($"Invalid char {b:x} at index {idx}");
            }
        }

        private void WriteInternal(byte[] bytes, MemoryStream? stream=null)
        {
            if (stream == null)
            {
                if (_rootEmptyStream.Position > 0)
                {
                    _stream.Write(_rootEmptyStream.ToArray());
                    _rootEmptyStream.SetLength(0);
                }
                stream = _stream;
            }

            if (_prevElement)
            {
                if (_groupDepth > 0 && !_firstInGroup)
                {
                    stream.WriteByte((byte)':');
                } else
                {
                    stream.WriteByte((byte)'+');
                }
            }
            _prevElement = true;
            _firstInGroup = false;
            stream.Write(bytes);
        }

        private void WriteInternal(string s, bool crLfAllowed=true)
        {
            WriteInternal(s.ToCharArray(), crLfAllowed);
        }

        private static readonly List<char> needingEscape = ['\'', '?', ':', '+'];

        private void WriteInternal(char[] charStr, bool crLfAllowed=true)
        {
            var numToBeEscaped = charStr.Where(needingEscape.Contains).Count();
            var escapedCharStr = new char[charStr.Length + numToBeEscaped];
            int j = 0;
            for (int i = 0;  i < escapedCharStr.Length; i++)
            {
                switch(charStr[j])
                {
                    case '\'':
                    case '?':
                    case ':':
                    case '+':
                        escapedCharStr[i++] = '?';
                        break;
                }
                escapedCharStr[i] = charStr[j++];
            }
            var bytes = Encoding.Latin1.GetBytes(escapedCharStr);
            CheckValidBytes(bytes, crLfAllowed);
            WriteInternal(bytes);
        }

        public void Write(string? s, FieldType type, int max_length = NO_LENGTH)
        {
            if (s == null)
            {
                WriteEmpty(); return;
            }

            if (max_length != NO_LENGTH && max_length < s.Length)
            {
                throw new ArgumentException($"\"{s}\" longer than {max_length} chars");
            }
            switch(type)
            {
                case FieldType.ALPHA_NUMERIC:
                case FieldType.CODE:
                    WriteInternal(s, crLfAllowed: false);
                    break;

                case FieldType.TEXT:
                    WriteInternal(s);
                    break;

                case FieldType.IDENTIFICATION:
                    Write(s, FieldType.ALPHA_NUMERIC, max_length: 30);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as string");
            }
        }

        public void Write(char[] str, FieldType type, int max_length = NO_LENGTH)
        {
            if (max_length != NO_LENGTH && max_length < str.Length)
            {
                throw new ArgumentException($"Char-String longer than {max_length} chars");
            }
            switch (type)
            {
                case FieldType.ALPHA_NUMERIC:
                    WriteInternal(str, crLfAllowed: false);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as char string");
            }
        }

        public void Write(bool? b,  FieldType type)
        {
            if (b == null)
            {
                WriteEmpty();
                return;
            }

            switch(type)
            {
                case FieldType.YES_NO:
                    if ((bool)b)
                    {
                        Write("J", FieldType.ALPHA_NUMERIC, max_length: 1);
                    } else
                    {
                        Write("N", FieldType.ALPHA_NUMERIC, max_length: 1);
                    }
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as bool");
            }
        }

        public void Write(int? i, FieldType type, int length=NO_LENGTH, int max_length = NO_LENGTH)
        {
            if (i == null)
            {
                WriteEmpty();
                return;
            }

            string lengthMarker = string.Empty;
            if (length != NO_LENGTH)
            {
                lengthMarker = length.ToString();
            }
            switch(type)
            {
                case FieldType.NUMERIC:
                    Write(i?.ToString("D"), FieldType.ALPHA_NUMERIC, max_length: max_length);
                    break;

                case FieldType.DIGITS:
                    Write(i?.ToString("D" + lengthMarker), FieldType.ALPHA_NUMERIC, max_length: max_length);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as int");
            }
        }

        public void Write(DateTime? datetime, FieldType type)
        {
            if (datetime == null)
            {
                WriteEmpty(); return;
            }
            switch(type)
            {
                case FieldType.DATE:
                    Write(DateOnly.FromDateTime((DateTime)datetime), type);
                    break;

                case FieldType.TIME:
                    Write(TimeOnly.FromDateTime((DateTime)datetime), type);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as time");
            }
        }

        public void Write(DateOnly? date, FieldType type)
        {
            if (date == null)
            {
                WriteEmpty(); return;
            }
            switch(type)
            {
                case FieldType.DATE:
                    Write(date?.ToString("yyyyMMdd"), FieldType.ALPHA_NUMERIC);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as date");
            }
        }

        public void Write(TimeOnly? time, FieldType type)
        {
            if (time == null)
            {
                WriteEmpty(); return;
            }
            switch (type)
            {
                case FieldType.TIME:
                    Write(time?.ToString("HHmmss"), FieldType.ALPHA_NUMERIC);
                    break;

                default:
                    throw new NotSupportedException(type.ToString() + " not supported as time");
            }
        }

        public void WriteEmpty()
        {
            WriteInternal([], _rootEmptyStream);
        }

        public void Write(byte[] bytes)
        {
            var ascii_len = bytes.Length.ToString("D");
            var prefix = Encoding.Latin1.GetBytes("@" + ascii_len + "@");
            WriteInternal(prefix);
            _stream.Write(bytes);
        }

        public void Clear()
        {
            _groupDepth = 0;
            _prevElement = false;
            _firstInGroup = true;

            _stream.SetLength(0);
            _rootEmptyStream.SetLength(0);
        }
    }
}
