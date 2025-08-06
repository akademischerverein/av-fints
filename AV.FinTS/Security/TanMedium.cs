using AV.FinTS.Raw.Codes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Security
{
    public class TanMedium
    {
        public string? Name { get; set; }

        public TanMediumClass Class { get; set; }

        public TanMediumStatus State { get; set; }

        public DateOnly? LastUsage { get; set; }

        public DateOnly? ActivatedOn { get; set; }
    }

    public class TanMediumResponse
    {
        public TanUsageOption UsageOption { get; set; }

        public List<TanMedium> TanMediums { get; set; } = new();
    }
}
