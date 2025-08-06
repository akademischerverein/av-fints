using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class Amount : IElementStructure
    {
        public decimal Value { get; set; }

        public string Currency { get; set; } = null!;

        public static Amount? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var val = reader.ReadDecimal();
            if (val == null)
            {
                reader.LeaveGroup();
                return null;
            }
            var amount = new Amount
            {
                Value = (decimal)val!,
                Currency = reader.Read()!
            };
            reader.LeaveGroup();
            return amount;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
