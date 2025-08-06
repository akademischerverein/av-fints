using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Models
{
    internal interface IMultiValue
    {
        internal bool CanBeConvertedTo(Type type);

        internal object ConvertTo(Type type);
    }
}
