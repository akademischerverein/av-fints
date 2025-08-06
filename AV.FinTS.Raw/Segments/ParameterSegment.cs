using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Segments
{
    public abstract class ParameterSegment : ISegment
    {
        public abstract SegmentId Head { get; protected set; }

        public int MaxOrders { get; set; }

        public int MinSignatures { get; set; }

        public int? SecurityClass { get; set; }

        private protected virtual void Read(MessageReader reader, bool legacy=false)
        {
            MaxOrders = (int)reader.ReadInt()!;
            MinSignatures = (int)reader.ReadInt()!;

            if (!legacy)
            {
                SecurityClass = reader.ReadInt();
            }
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
