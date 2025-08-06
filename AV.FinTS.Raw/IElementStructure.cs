using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw
{
    public interface IElementStructure
    {
        public void Write(MessageWriter writer);
    }
}
