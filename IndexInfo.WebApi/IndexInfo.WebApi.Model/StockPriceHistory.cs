using System;
using System.ComponentModel.DataAnnotations;

namespace IndexInfo.WebApi.Model
{
    public class StockPriceHistory
    {
        [Key]
        public DateTime Time { get; set; }
        public decimal Price { get; set; }
    }
}
