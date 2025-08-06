using System;

namespace AV.FinTS.Raw.Structures
{
    public class SegmentId
    {
        public string Name { get; set; } = null!;

        public int Version { get; set; }

        public int? Number { get; set; }

        public int? SegmentReference { get; set; }

        public bool Equals(SegmentId? other)
        {
            return other is not null && other.Name == Name && other.Version == Version;
        }

        public override bool Equals(object? obj)
        {
            return obj is SegmentId && Equals(obj as SegmentId);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Name, Version);
        }

        public override string ToString()
        {
            return Name + ":" + Version;
        }

        public bool IsParameter => Name.Length == 6 && Name[1] == 'I' && Name[5] == 'S';

        public bool IsBpd => IsParameter || Name == "HIBPA" || Name == "HIKOM" || Name == "HISHV" || Name == "HIKPV";

        public static bool operator==(SegmentId? left, SegmentId? right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(SegmentId? left, SegmentId? right)
        {
            if (left is null) return right is not null;
            return !left.Equals(right);
        }
    }
}
