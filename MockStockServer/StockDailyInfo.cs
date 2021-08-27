using System;
using System.Collections.Generic;
using System.Text;

namespace MockStockServer
{
    public class StockDailyInfo
    {
        public DateTime DateStamp { get; set; }
        public decimal open { get; set; }
        public decimal high { get; set; }
        public decimal low { get; set; }
        public decimal close { get; set; }
        public decimal volume { get; set; }
    }
}
