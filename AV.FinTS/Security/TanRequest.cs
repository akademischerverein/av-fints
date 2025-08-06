using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Security
{
    public class TanRequest
    {
        public string? Challenge { get; internal set; }

        public byte[] ChallengeHhdUc { get; internal set; } = [];

        public DateTime? ValidUpTo { get; internal set; }

        public string Reference { get; internal set; } = null!;

        public string? TanMedium { get; internal set; }
    }
}
