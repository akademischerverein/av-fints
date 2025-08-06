using AV.FinTS.Formats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AV.FinTS.Models
{
    public class TransactionsResponse
    {
        public SepaAccount Account { get; set; } = null!;

        public DateOnly From { get; set; }

        public DateOnly To { get; set; }

        public SwiftStatement? BookedTransactions { get; set; }
    }
}
