using AV.FinTS.Raw.Segments.Auth;
using AV.FinTS.Raw.Segments.Feedback;
using AV.FinTS.Raw.Segments.Internal;
using AV.FinTS.Raw.Segments.ParameterData;
using AV.FinTS.Raw.Segments.Sepa;
using AV.FinTS.Raw.Segments.Transactions;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public static class MessageParser
    {
        private static Dictionary<SegmentId, Func<MessageReader, SegmentId, ISegment>> registeredSegments = new();

        private static bool _segmentsLoaded = false;

        static MessageParser()
        {
            if (_segmentsLoaded) return;

            RegisterType(typeof(HIBPA3));
            RegisterType(typeof(HIEKA3));
            RegisterType(typeof(HIEKA4));
            RegisterType(typeof(HIEKA5));
            RegisterType(typeof(HIEKAS3));
            RegisterType(typeof(HIEKAS4));
            RegisterType(typeof(HIEKAS5));
            RegisterType(typeof(HIKAZ4));
            RegisterType(typeof(HIKAZ5));
            RegisterType(typeof(HIKAZ6));
            RegisterType(typeof(HIKAZ7));
            RegisterType(typeof(HIKAZS4));
            RegisterType(typeof(HIKAZS5));
            RegisterType(typeof(HIKAZS6));
            RegisterType(typeof(HIKAZS7));
            RegisterType(typeof(HIKOM4));
            RegisterType(typeof(HIPINS1));
            RegisterType(typeof(HIRMG2));
            RegisterType(typeof(HIRMS2));
            RegisterType(typeof(HISPA1));
            RegisterType(typeof(HISPA2));
            RegisterType(typeof(HISPAS1));
            RegisterType(typeof(HISPAS2));
            RegisterType(typeof(HISYN4));
            RegisterType(typeof(HITAB4));
            //RegisterType(typeof(HITAB5));
            RegisterType(typeof(HITABS4));
            //RegisterType(typeof(HITABS5));
            RegisterType(typeof(HITAN6));
            RegisterType(typeof(HITAN7));
            RegisterType(typeof(HITANS6));
            RegisterType(typeof(HITANS7));
            RegisterType(typeof(HIUPA4));
            RegisterType(typeof(HIUPD6));
            RegisterType(typeof(HNHBK3));
            RegisterType(typeof(HNHBS1));
            RegisterType(typeof(HNVSD1));
            RegisterType(typeof(HNVSK3));

            _segmentsLoaded = true;
        }

        private static void RegisterType(Type t)
        {
            var c = t.GetConstructor(Type.EmptyTypes);
            var seg = (ISegment)c!.Invoke([]);

            var readMethod = t.GetMethod("Read", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, [typeof(MessageReader), typeof(SegmentId)]);

            if (readMethod == null || readMethod.ReturnType != typeof(ISegment))
            {
                throw new ArgumentException();
            }

            var func = (Func<MessageReader, SegmentId, ISegment>)Delegate.CreateDelegate(registeredSegments.GetType().GenericTypeArguments[1], readMethod);
            registeredSegments.TryAdd(seg.Head, func);
        }

        public static IReadOnlyList<ISegment> ParseMessage(byte[] data)
        {
            var segments = new List<ISegment>();
            var reader = new MessageReader(data);

            while (reader.Position < data.Length)
            {
                reader.StartNextSegment();
                var segId = reader.ReadSegmentHeader();
                if (registeredSegments.ContainsKey(segId))
                {
                    segments.Add(registeredSegments[segId](reader, segId));
                } else
                {
                    reader.SkipCurrentSegment();
                }
            }

            return segments;
        }
    }
}
