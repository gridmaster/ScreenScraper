using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace ScreenScraper
{
    public class ScreenScraperContext : DbContext
    {
        public DbSet<InsiderTransaction> InsiderTransactions { get; set; }

    }
}
