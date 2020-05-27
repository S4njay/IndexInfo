using System.Threading.Tasks;
using IndexInfo.WebApi.Model;
using System.Net.Http;
using System.Text.Json;
using System.Collections.Generic;

namespace IndexInfo.WebApi.Services
{
    public class StockPricesService : IStockPricesService
    {
        public async Task<StockPrice> GetStockPriceFromOutboundApi(string id)
        {
            StockPrice stockPrice;
            using (var client = new HttpClient())
            {
                // TODO: move to config
                var streamAsync = await client.GetStreamAsync(
                    $"http://host.docker.internal:5000/api/v1/quotes?id={id}");

                stockPrice = await JsonSerializer
                .DeserializeAsync<StockPrice>(streamAsync);
            }

            return stockPrice;
        }

        public Task<IEnumerable<StockPriceHistory>> GetStockPriceHistoryFromOutboundApi(string id, double bid)
        {
            throw new System.NotImplementedException();
        }

        public async Task SendUpdateStockPriceCommand(IServiceBus bus,string id, StockPrice existingStockPrice)
        {
            await bus.SendCommandAsync(new AzureUpdateStockPriceCommand(id));
        }

        public Task SendUpdateStockPriceHistoryCommand(IServiceBus _bus, string id, StockPriceHistory stockPrice)
        {
            throw new System.NotImplementedException();
        }
    }
}