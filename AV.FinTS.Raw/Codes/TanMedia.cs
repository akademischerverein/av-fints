using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Codes
{
    public enum TanMediumType
    {
        All = 0,
        Active = 1,
        Available = 2
    }

    public enum TanMediumClass
    {
        ALL,
        LIST,
        TAN_GENERATOR,
        MOBILE_PHONE,
        SECODER,
        BILATERAL
    }

    public enum TanUsageOption
    {
        ALL_ACTIVE_PARALLEL = 0,
        ONE_AT_A_TIME = 1,
        ONE_MOBILE_PHONE_AND_GENERATOR_PARALLEL = 2
    }

    public enum TanMediumStatus
    {
        ACTIVE = 1,
        AVAILABLE = 2,
        ACTIVE_FOLLOW_UP = 3,
        AVAILABLE_FOLLOW_UP = 4
    }
}
