using System.Threading.Tasks;
using IndexInfo.WebApi.Model;
using System.Net.Http;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Text;

namespace IndexInfo.WebApi.Services
{
    public class StockPricesService : IStockPricesService
    {
        public double RefreshInterval => 60 * 5;

        public async Task<StockPrice> GetStockPriceFromOutboundApi(string id)
        {
            StockPrice stockPrice;
            using (var client = new HttpClient())
            {
                // TODO: move to config
                var streamAsync = await client.GetStreamAsync(
                    $"http://localhost:5002/api/v1/quotes?id={id}");

                stockPrice = await System.Text.Json.JsonSerializer
                .DeserializeAsync<StockPrice>(streamAsync);
                stockPrice.longName = stockPrice.longName ?? stockPrice.symbol;
            }

            return stockPrice;
        }

        public async Task<IEnumerable<StockPriceHistory>> GetStockPriceHistoryFromOutboundApi(string id, 
        double bid)
        {
            IEnumerable<StockPriceHistory> stockPriceHistory;
            using (var client = new HttpClient())
            {
                // TODO: move to config
                var streamAsync = await client.GetStreamAsync(
                    $"http://localhost:5002/api/v1/history?id={id}");

                stockPriceHistory = await System.Text.Json.JsonSerializer
                .DeserializeAsync<IEnumerable<StockPriceHistory>>(streamAsync);
            }
            return stockPriceHistory;
        }

        public async Task SendUpdateStockPriceCommand(IServiceBus bus, string id, StockPrice existingStockPrice)
        {
            // await bus.SendCommandAsync(new AzureUpdateStockPriceCommand(id));
            var newStockPrice = await GetStockPriceFromOutboundApi(id);

            var newStockPriceJson = JsonConvert.SerializeObject(newStockPrice);
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    // TODO: move to config
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    await client.PutAsync($"https://localhost:5001/api/StockPrices/{id}",
                    new StringContent(newStockPriceJson, Encoding.UTF8, "application/json"));
                }
            }
        }

        public async Task SendUpdateStockPriceHistoryCommand(IServiceBus _bus, string id, StockPriceHistory stockPriceHistory)
        {
            var newStockPrice = await GetStockPriceHistoryFromOutboundApi(id, 
                                stockPriceHistory.Close.Value);

            var newStockPriceJson = JsonConvert.SerializeObject(newStockPrice);
            using (var handler = new HttpClientHandler())
            {
                using (var client = new HttpClient(handler))
                {
                    // TODO: move to config
                    handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };

                    await client.PutAsync($"https://localhost:5001/api/StockPricesHistory/{id}",
                    new StringContent(newStockPriceJson, Encoding.UTF8, "application/json"));
                }
            }
        }
    }
}
