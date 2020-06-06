using System;
using System.ComponentModel.DataAnnotations;

namespace IndexInfo.WebApi.Model
{
    [Serializable]
    public class StockPriceHistory
    {
        [Key]
        public Guid StockPriceHistoryId {get; set; }
        public string symbol {get; set; }
        public double? date { get; set; }
        public double? Open { get; set; }
        public double? High { get; set; }
        public double? Low { get; set; }
        public double? Close { get; set; }
        public double? Volume { get; set; }
        public double? Dividends { get; set; }
        public double? StockSplits { get; set; }
    }
}
