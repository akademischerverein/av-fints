using AV.FinTS.Raw;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Helper
{
    public static class RawMessageHelpers
    {
        public static ISegment Get(this RawMessage msg, string seg, uint idx = 0)
        {
            return msg.Segments.Where(s => s.Head.Name == seg).Skip((int)idx).First();
        }

        public static ISegment Get(this RawMessage msg, string seg, int ver, uint idx = 0)
        {
            return msg.Segments.Where(s => s.Head.Name == seg && s.Head.Version == ver).Skip((int)idx).First();
        }

        public static T Get<T>(this RawMessage msg, uint idx = 0) where T : ISegment
        {
            var tseg = typeof(T).GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>()) as ISegment;

            var s = msg.Get(tseg!.Head.Name, tseg!.Head.Version, idx);
            if (s is not T)
            {
                throw new InvalidCastException(typeof(T).Name + " != " + s.GetType().Name);
            }
            return (T)s;
        }

        public static IReadOnlyCollection<ISegment> GetAll(this RawMessage msg, string seg)
        {
            return msg.Segments.Where(s => s.Head.Name == seg).ToList();
        }

        public static IReadOnlyCollection<ISegment> GetAll(this RawMessage msg, string seg, int ver)
        {
            return msg.Segments.Where(s => s.Head.Name == seg && s.Head.Version == ver).ToList();
        }

        public static IReadOnlyCollection<T> GetAll<T>(this RawMessage msg) where T : ISegment
        {
            var tseg = typeof(T).GetConstructor(Array.Empty<Type>())!.Invoke(Array.Empty<object>()) as ISegment;

            var s = msg.GetAll(tseg!.Head.Name, tseg!.Head.Version);
            if (s.Any(seg => seg is not T))
            {
                throw new InvalidCastException(typeof(T).Name + " != " + s.GetType().Name);
            }
            return s.Select(seg => (T)seg).ToList();
        }
    }
}
