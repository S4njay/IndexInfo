using Microsoft.EntityFrameworkCore;

namespace IndexInfo.WebApi.Model
{
    public class StockContext : DbContext
    {
        public StockContext(DbContextOptions<StockContext> options) : base(options)
        {

        }

        public DbSet<StockPrice> StockPrices {get;set;}
        public DbSet<StockPriceHistory> StockPricesHistory {get;set;}
    }
}