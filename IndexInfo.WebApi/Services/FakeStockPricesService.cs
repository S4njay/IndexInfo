using System.Threading.Tasks;
using IndexInfo.WebApi.Model;
using System.Net.Http;
using System.Text.Json;
using System;
using RandomNameGeneratorLibrary;
using System.Text;
using Newtonsoft.Json;

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
            var bid = Math.Round((double)new Random().Next(1,10000) / 10, 2);

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

        public async Task SendUpdateStockPriceCommand(IServiceBus bus,string id, StockPrice existingStockPrice)
        {
            var newStockPrice = existingStockPrice;
            newStockPrice.bid = Math.Round((double)(existingStockPrice.bid + (double)((double)new Random().Next(-10, 10) / (double)1000.0) 
                                * existingStockPrice.bid), 2);
            
            var newStockPriceJson = JsonConvert.SerializeObject(newStockPrice);
            using(var handler = new HttpClientHandler())
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
    }
}
