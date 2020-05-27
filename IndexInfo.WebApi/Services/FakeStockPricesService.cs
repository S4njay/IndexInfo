using System.Threading.Tasks;
using IndexInfo.WebApi.Model;
using System.Net.Http;
using System.Text.Json;
using System;
using RandomNameGeneratorLibrary;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;

namespace IndexInfo.WebApi.Services
{
    public class FakeStockPricesService : IStockPricesService
    {
        private readonly PlaceNameGenerator _placeNamegen;

        public FakeStockPricesService(PlaceNameGenerator placeNamegen)
        {
            _placeNamegen = placeNamegen;
        }
        public async Task<StockPrice> GetStockPriceFromOutboundApi(string id)
        {
            var bid = Math.Round((double)new Random().Next(1, 10000) / 10, 2);

            var stockPrice = new StockPrice
            {
                bid = bid,
                dateUpdated = DateTime.Now,
                longName = _placeNamegen.GenerateRandomPlaceName(),
                previousClose = bid,
                regularMarketPreviousClose = bid,
                symbol = id            
            };

            return await Task.FromResult(stockPrice);
        }

        public Task<IEnumerable<StockPriceHistory>> GetStockPriceHistoryFromOutboundApi(string id, double bid)
        {
            var stockPriceHistory = Enumerable.Range(0, 500)
                    .Select(x =>
                    {
                        var close = Math.Round((double)(bid +
                                (double)(x * (double)new Random().Next(-10, 10) / (double)1000.0)
                                * bid), 2);
                        var s = new StockPriceHistory
                        {
                            StockPriceHistoryId = Guid.NewGuid(),
                            symbol = id,
                            Date = DateTime.Now.Date.AddDays(-1 * x),
                            Close = close,
                            Dividends = 0,
                            High = close * 1.01,
                            Low = close * 0.98,
                            Open = close * 0.99,
                            StockSplits = 0
                        };

                        return s;
                    });
            
            return Task.FromResult(stockPriceHistory);
        }

        public async Task SendUpdateStockPriceCommand(IServiceBus bus, string id, StockPrice existingStockPrice)
        {
            var newStockPrice = existingStockPrice;
            newStockPrice.bid = Math.Round((double)(existingStockPrice.bid + (double)((double)new Random().Next(-10, 10) / (double)1000.0)
                                * existingStockPrice.bid), 2);

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

        public async Task SendUpdateStockPriceHistoryCommand(IServiceBus _bus, string id, StockPriceHistory stockPrice)
        {
            var newStockPrice = stockPrice;
            newStockPrice.Close = Math.Round((double)(stockPrice.Close + (double)((double)new Random().Next(-10, 10) / (double)1000.0)
                                * stockPrice.Close), 2);

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
