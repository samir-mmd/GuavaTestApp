using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuavaStart.Messages
{
    public class TransactionResult
    {
        public int userId { get; set; }
        public string stockID { get; set; }
        public decimal maxPrice { get; set; }
        public string result { get; set; }
    }
}
