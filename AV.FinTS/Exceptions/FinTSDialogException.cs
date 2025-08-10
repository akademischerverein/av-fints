using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Exceptions
{
    public class FinTSDialogException : FinTSException
    {
        internal FinTSDialogException() : base() { }

        internal FinTSDialogException(string message) : base(message) { }

        internal FinTSDialogException(string message, ICollection<Feedback> fbs) : base(message)
        {
            Feedbacks = (IReadOnlyCollection<Feedback>)fbs;
        }

        public IReadOnlyCollection<Feedback> Feedbacks { get; } = [];
    }
}
