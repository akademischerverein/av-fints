using AV.FinTS.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Security
{
    public sealed class TanResponse
    {
        internal bool IsDecoupled { get; private set; }

        internal bool IsCanceled { get; private set; }

        internal string Tan { get; private set; } = null!;

        internal void Verify(bool decoupled)
        {
            if (IsCanceled)
            {
                throw new OperationCancelledException();
            }

            if (!decoupled && IsDecoupled)
            {
                throw new InvalidOperationException("can't make status request for non decoupled auth methods");
            }

            if (!IsDecoupled && Tan == null)
            {
                throw new InvalidDataException("Tan can't be null");
            }
        }

        public static TanResponse Cancelled()
        {
            return new TanResponse
            {
                IsDecoupled = false,
                IsCanceled = true
            };
        }

        public static TanResponse Decoupled()
        {
            return new TanResponse
            {
                IsDecoupled = true,
                IsCanceled = false
            };
        }

        public static TanResponse WithTan(string tan)
        {
            return new TanResponse
            {
                IsCanceled = false,
                Tan = tan,
                IsDecoupled = false
            };
        }
    }
}
