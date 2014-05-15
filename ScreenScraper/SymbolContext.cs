using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraper
{
    class SymbolContext : DbContext
    {
        public SymbolContext()
            : base("SymbolContext")
        {
        }

        //public DbSet<Log> Logs { get; set; }

        //public DbSet<TickerSymbol> TickerSymbols { get; set; }
    }
}
