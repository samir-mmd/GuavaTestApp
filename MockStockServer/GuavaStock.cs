using System;
using System.Collections.Generic;
using System.Text;

namespace MockStockServer
{
    public class GuavaStock
    {
        public int Id { get; set; }
        public int UserID { get; set; }
        public string StockId { get; set; }
        public string StockName { get; set; }
        public string StockSymbol { get; set; }
        public DateTime OperationDate { get; set; }
        public decimal OperationPrice { get; set; }
        public decimal OperationVolume { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Profit { get; set; }
    }
}
