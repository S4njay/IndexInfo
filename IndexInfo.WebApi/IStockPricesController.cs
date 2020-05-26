using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using IndexInfo.WebApi.Model;

namespace IndexInfo.WebApi
{
    public interface IStockPricesService
    {
        Task<StockPrice> GetStockPriceFromOutboundApi(string id);
        Task SendUpdateStockPriceCommand(IServiceBus _bus, string id, StockPrice stockPrice);
    }
}
