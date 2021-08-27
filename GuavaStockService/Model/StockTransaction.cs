using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaStockService.Model
{
    public class StockTransaction
    {
        public int userId { get; set; }
        public string stockID { get; set; }
        public decimal maxPrice { get; set; }
        public string hookURL { get; set; }
    }
}
