using AV.FinTS.Raw;
using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Helpers
{
    internal static class ISegmentHelpers
    {
        internal static object? GetProperty(this ISegment seg, string property)
        {
            var prop = seg.GetType().GetProperty(property, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var props = seg.GetType().GetProperties();
            if (prop == null)
            {
                throw new MissingMemberException(property);
            }
            return prop.GetValue(seg);
        }

        internal static object GetPropertyNotNull(this ISegment seg, string property)
        {
            var ret = GetProperty(seg, property);
            if (ret == null)
            {
                throw new NullReferenceException(property);
            }
            return ret;
        }

        internal static T? GetProperty<T>(this ISegment seg, string property) where T : class
        {
            var ret = GetProperty(seg, property);
            if (ret == null)
            {
                return null;
            }
            return (T?)ret;
        }

        internal static T? GetPropertyValue<T>(this ISegment seg, string property) where T : struct
        {
            var ret = GetProperty(seg, property);
            if (ret == null)
            {
                return null;
            }
            return (T?)ret;
        }

        internal static T GetPropertyNotNull<T>(this ISegment seg, string property)
        {
            var ret = GetPropertyNotNull(seg, property);
            return (T)ret;
        }

        internal static string GetCustomerSegmentName(this SegmentId segmentId)
        {
            return $"{segmentId.Name[0]}K{segmentId.Name.Substring(2, 3)}";
        }

        internal static string GetBankSegmentName(this SegmentId segmentId)
        {
            return $"{segmentId.Name[0]}I{segmentId.Name.Substring(2, 3)}";
        }

        internal static string GetParameterSegmentName(this SegmentId segmentId)
        {
            return $"{segmentId.Name[0]}I{segmentId.Name.Substring(2, 3)}S";
        }
    }
}
