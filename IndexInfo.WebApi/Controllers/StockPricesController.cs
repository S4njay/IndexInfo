using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IndexInfo.WebApi.Model;
using System.Net.Http;
using System.Text.Json;

namespace IndexInfo.WebApi.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class StockPricesController : ControllerBase
    {
        private readonly StockContext _context;
        private readonly IServiceBus _bus;

        private readonly IStockPricesService _stockPricesService;


        public StockPricesController(StockContext context,
                                    IServiceBus bus,
                                    IStockPricesService stockPricesService)
        {
            _context = context;
            _bus = bus;
            _stockPricesService = stockPricesService;
        }

        // GET: api/StockPrices
        [HttpGet]
        public async Task<ActionResult<IEnumerable<StockPrice>>> GetStockPrices()
        {
            return await _context.StockPrices.ToListAsync();
        }

        // GET: api/StockPrices/5
        [HttpGet("{id}")]
        public async Task<ActionResult<StockPrice>> GetStockPrice(string id)
        {
            var stockPrice = await _context.StockPrices.FindAsync(id);

            if (stockPrice == null)
            {
                try
                {
                    System.Console.WriteLine($"Stock Price for {id} not found in local store.");
                    System.Console.WriteLine($"Making flight to outbound api to get stock price for {id}");
                    StockPrice createStockPrice = null;
                    createStockPrice = await _stockPricesService.GetStockPriceFromOutboundApi(id);

                    return await PostStockPrice(createStockPrice);
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                    return NotFound();
                }
            }

            //TODO: Move To config

            if (stockPrice.dateUpdated < DateTime.Now.AddSeconds(-5))
            {
                System.Console.WriteLine($"Local stock stock price for {id} is old.");
                System.Console.WriteLine($"Sending an update stock price command to service for {id}");

                await _stockPricesService.SendUpdateStockPriceCommand(_bus, id, stockPrice);
                stockPrice.dateUpdated = DateTime.Now;
                await PutStockPrice(id, stockPrice);
            }
            else
            {
                System.Console.WriteLine($"Returning local store value for {id}");
            }
            return stockPrice;
        }

        

        // PUT: api/StockPrices/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutStockPrice(string id, StockPrice stockPrice)
        {
            Console.WriteLine($"Received stock price update request for : ${id}");
            stockPrice.dateUpdated = DateTime.Now;

            if (id != stockPrice.symbol)
            {
                return BadRequest();
            }

            _context.Entry(stockPrice).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                Console.WriteLine($"Updated stock price: ${id}");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StockPriceExists(id))
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

        // POST: api/StockPrices
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<StockPrice>> PostStockPrice(StockPrice stockPrice)
        {
            stockPrice.dateUpdated = DateTime.Now;

            _context.StockPrices.Add(stockPrice);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StockPriceExists(stockPrice.symbol))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }

            return CreatedAtAction("GetStockPrice", new { id = stockPrice.symbol }, stockPrice);
        }

        // DELETE: api/StockPrices/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<StockPrice>> DeleteStockPrice(string id)
        {
            var stockPrice = await _context.StockPrices.FindAsync(id);
            if (stockPrice == null)
            {
                return NotFound();
            }

            _context.StockPrices.Remove(stockPrice);
            await _context.SaveChangesAsync();

            return stockPrice;
        }

        private bool StockPriceExists(string id)
        {
            return _context.StockPrices.Any(e => e.symbol == id);
        }
    }
}
