using AV.FinTS.Raw.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Exceptions
{
    public class FinTSAuthException : FinTSException
    {
        internal FinTSAuthException() : base() { }

        internal FinTSAuthException(string message) : base(message) { }

        internal FinTSAuthException(string message, ICollection<Feedback> fbs) : base(message)
        {
            Feedbacks = (IReadOnlyCollection<Feedback>)fbs;
        }

        public IReadOnlyCollection<Feedback> Feedbacks { get; } = [];
    }
}
