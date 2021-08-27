using System;
using System.Collections.Generic;
using System.Text;

namespace MockStockServer
{
    class StockBase
    {
        public string Id { get; set; }
        public string Information { get; set; }
        public string Symbol { get; set; }
        public DateTime LastRefreshed { get; set; }
        public decimal CurrentPrice { get; set; }
        public List<StockDailyInfo> dailyInfos = new List<StockDailyInfo>();

        public StockBase(string id,string info, string symbol)
        {
            Information = info;
            Symbol = symbol;
            Id = id+info+symbol;
        }
    }
}
