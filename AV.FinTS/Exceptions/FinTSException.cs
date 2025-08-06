using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Exceptions
{
    public class FinTSException : Exception
    {
        internal FinTSException() : base() { }

        internal FinTSException(string message) : base(message) { }
    }
}
