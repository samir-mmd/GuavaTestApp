using GuavaStart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuavaStart.Messages
{
    public class SellingStockTransaction
    {
        public int userId { get; set; }
        public string stockID { get; set; }
        public int holdID { get; set; }
        public decimal maxPrice { get; set; }
        public string hookURL { get; set; }
    }
}
