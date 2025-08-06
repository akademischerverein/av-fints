using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static AV.FinTS.Raw.Structures.AccountLimit;

namespace AV.FinTS.Raw.Structures
{
    public class AllowedOperation : IElementStructure
    {
        public string Operation { get; set; } = null!;

        public int NumRequiredSignatures { get; set; }

        public LimitType? LimitType { get; set; }

        public Amount? LimitAmount { get; set; }

        public int? LimitDays { get; set; }

        public static AllowedOperation? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var strOp = reader.Read();
            if (strOp == null)
            {
                reader.LeaveGroup();
                return null;
            }

            var op = new AllowedOperation
            {
                Operation = strOp!,
                NumRequiredSignatures = (int)reader.ReadInt()!
            };

            if (reader.GroupEnded)
            {
                reader.LeaveGroup();
                return op;
            }

            var strType = reader.Read();
            LimitType type;
            switch (strType)
            {
                case "E":
                    type = AccountLimit.LimitType.Order;
                    break;

                case "T":
                    type = AccountLimit.LimitType.Daily;
                    break;

                case "W":
                    type = AccountLimit.LimitType.Weekly;
                    break;

                case "M":
                    type = AccountLimit.LimitType.Monthly;
                    break;

                case "Z":
                    type = AccountLimit.LimitType.TimeBased;
                    break;

                default:
                    throw new InvalidDataException("Unknown LimitType");
            }
            op.LimitType = type;
            op.LimitAmount = Amount.Read(reader);
            op.LimitDays = reader.ReadInt();
            reader.LeaveGroup();
            return op;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
