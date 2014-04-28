using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraper
{
    public class InsiderTransactions
    {
        public string Purchases { get; set; }
        public string Sales { get; set; }
        public string NetSharesPurchasedSold { get; set; }
        public string TotalInsiderSharesHeld { get; set; }
        public string PercentNetSharesPurchasedSold { get; set; }
        public string InstitutioNetSharePurchasedSold { get; set; }
        public string InstitutionPercentChangeInSharesHeld { get; set; }
    }
}
