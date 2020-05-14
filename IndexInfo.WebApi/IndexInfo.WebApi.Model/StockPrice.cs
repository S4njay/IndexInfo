using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IndexInfo.WebApi.Model
{
    public class StockPrice
    {
        [Key]
        public string symbol { get; set; }
        public double? bid { get; set; }
        public string longName { get; set; }
        public double? regularMarketPreviousClose {get;set;}
        public double? previousClose {get;set;}
        public DateTime dateUpdated { get; set; }
    }
}
