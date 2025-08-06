using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Exceptions
{
    public class FinTSDialogInitException : FinTSException
    {
        internal FinTSDialogInitException(string message) : base(message) { }
    }
}
