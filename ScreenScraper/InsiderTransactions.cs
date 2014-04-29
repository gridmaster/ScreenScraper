using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScreenScraper
{
    public class InsiderTransactions
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public int SymbolId { get; set; }
        public string PurchasesShares { get; set; }
        public string PurchasesTrans { get; set; }
        public string SalesShares { get; set; }
        public string SalesTrans { get; set; }
        public string NetSharesPurchasedSoldShares { get; set; }
        public string NetSharesPurchasedSoldTrans { get; set; }
        public string TotalInsiderSharesHeldShares { get; set; }
        public string TotalInsiderSharesHeldTrans { get; set; }
        public string PercentNetSharesPurchasedSoldShares { get; set; }
        public string PercentNetSharesPurchasedSoldTrans { get; set; }
        public string InstitutioNetSharePurchasedSold { get; set; }
        public string InstitutionPercentChangeInSharesHeld { get; set; }
    }
}
