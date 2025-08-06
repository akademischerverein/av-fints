using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Raw.Structures
{
    public class AccountLimit : IElementStructure
    {
        public LimitType Type { get; set; }

        public Amount? Amount { get; set; }

        public int? LimitDays { get; set; }

        public static AccountLimit? Read(MessageReader reader)
        {
            reader.EnterGroup();
            var strType = reader.Read();
            if (strType == null)
            {
                reader.LeaveGroup();
                return null;
            }
            LimitType type;
            switch(strType)
            {
                case "E":
                    type = LimitType.Order;
                    break;

                case "T":
                    type = LimitType.Daily;
                    break;

                case "W":
                    type = LimitType.Weekly;
                    break;

                case "M":
                    type = LimitType.Monthly;
                    break;

                case "Z":
                    type = LimitType.TimeBased;
                    break;

                default:
                    throw new InvalidDataException("Unknown LimitType");
            }

            var limit = new AccountLimit
            {
                Type = type,
                Amount = Amount.Read(reader),
                LimitDays = reader.ReadInt()
            };
            reader.LeaveGroup();
            return limit;
        }

        public void Write(MessageWriter writer)
        {
            throw new NotImplementedException();
        }

        public enum LimitType
        {
            Order,
            Daily,
            Weekly,
            Monthly,
            TimeBased
        }
    }
}
