using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IndexInfo.WebApi.Model;
using System;

namespace IndexInfo.WebApi
{
    public interface IStockPricesService
    {
        Double RefreshInterval { get; }
        Task<StockPrice> GetStockPriceFromOutboundApi(string id);
        Task<IEnumerable<StockPriceHistory>> GetStockPriceHistoryFromOutboundApi(string id, double bid);
        Task SendUpdateStockPriceCommand(IServiceBus _bus, string id, StockPrice stockPrice);
        Task SendUpdateStockPriceHistoryCommand(IServiceBus _bus, string id, StockPriceHistory stockPrice);
    }
}
