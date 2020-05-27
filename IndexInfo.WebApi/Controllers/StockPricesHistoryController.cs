using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndexInfo.WebApi.Model;

namespace IndexInfo.WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StockPricesHistoryController : ControllerBase
    {
        private readonly StockContext _context;
        private readonly IServiceBus _bus;

        private readonly IStockPricesService _stockPricesService;


        public StockPricesHistoryController(StockContext context,
                                    IServiceBus bus,
                                    IStockPricesService stockPricesService)
        {
            _context = context;
            _bus = bus;
            _stockPricesService = stockPricesService;
        }        

        // GET: api/StockPricesHistory/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<StockPriceHistory>>> GetStockPricesHistory(string id)
        {
            var stockPriceHistory = await _context.StockPricesHistory.Where(x => x.symbol == id)
                                    .ToListAsync();
            var lastBid = await _stockPricesService.GetStockPriceFromOutboundApi(id);


            if (stockPriceHistory.Count() == 0)
            {
                try
                {
                    System.Console.WriteLine($"Stock Price history for {id} not found in local store.");
                    System.Console.WriteLine($"Making flight to outbound api to get stock price for {id}");
                    
                    IEnumerable<StockPriceHistory> createStockPriceHistory = null;
                    createStockPriceHistory = await _stockPricesService
                        .GetStockPriceHistoryFromOutboundApi(id, lastBid.bid.Value);

                    return await PostStockPriceHistory(createStockPriceHistory);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return NotFound();
                }
            }

            //TODO: Move To config

            var lastStockPriceHistory = stockPriceHistory.OrderByDescending(x => x.Date).FirstOrDefault();
            if (lastStockPriceHistory.Date < DateTime.Today)
            {
                System.Console.WriteLine($"Local stock stock price history for {id} is old.");
                System.Console.WriteLine($"Sending an update stock price history command to service for {id}");

                await _stockPricesService.SendUpdateStockPriceHistoryCommand(_bus, id, lastStockPriceHistory);
            }
            else
            {
                System.Console.WriteLine($"Returning local store history value for {id}");
            }
            return stockPriceHistory;
        }

        // POST: api/StockPricesHistory
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<IEnumerable<StockPriceHistory>>> PostStockPriceHistory(
            IEnumerable<StockPriceHistory> stockPriceHistory)
        {
            _context.StockPricesHistory.AddRange(stockPriceHistory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StockPriceHistoryExists(stockPriceHistory.First().symbol))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStockPricesHistory", new { id = stockPriceHistory.First().symbol }, stockPriceHistory);
        }

        // PUT: api/StockPricesHistory/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockPrice(string id, StockPriceHistory stockPriceHistory)
        {
            Console.WriteLine($"Received stock price history update request for : ${id}");

            if (id != stockPriceHistory.symbol)
            {
                return BadRequest();
            }

            _context.StockPricesHistory.Add(stockPriceHistory);
            
            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated stock price: ${id}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockPriceHistoryExists(id, stockPriceHistory.Date))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        private bool StockPriceHistoryExists(string id)
        {
            return _context.StockPrices.Any(e => e.symbol == id);
        }

        private bool StockPriceHistoryExists(string id, DateTime date)
        {
            return _context.StockPricesHistory.Any(e => e.symbol == id && e.Date == date);
        }
    }
}
