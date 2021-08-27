using System;
using System.Collections.Generic;
using System.Text;

namespace MockStockServer
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
