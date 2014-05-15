using System;
using System.Globalization;
using Newtonsoft.Json;

namespace ScreenScraper.Models
{
    internal class WCFSector
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "WCFXferDate")]
        public string WCFXferDate { get; set; }
        [JsonProperty(PropertyName = "Name")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "OneDayPriceChgPerCent")]
        public decimal OneDayPriceChgPerCent { get; set; }
        [JsonProperty(PropertyName = "MarketCap")]
        public string MarketCap { get; set; }
        [JsonProperty(PropertyName = "PriceToEarnings")]
        public decimal PriceToEarnings { get; set; }
        [JsonProperty(PropertyName = "ROEPerCent")]
        public decimal ROEPerCent { get; set; }
        [JsonProperty(PropertyName = "DivYieldPerCent")]
        public decimal DivYieldPerCent { get; set; }
        [JsonProperty(PropertyName = "DebtToEquity")]
        public decimal DebtToEquity { get; set; }
        [JsonProperty(PropertyName = "PriceToBook")]
        public decimal PriceToBook { get; set; }
        [JsonProperty(PropertyName = "NetProfitMarginMrq")]
        public decimal NetProfitMarginMrq { get; set; }
        [JsonProperty(PropertyName = "PriceToFreeCashFlowMrq")]
        public decimal PriceToFreeCashFlowMrq { get; set; }

        internal WCFSector()
        {
        }

        internal WCFSector(Sector sector)
        {
            Id = sector.Id;
            WCFXferDate = sector.Date.ToString(CultureInfo.InvariantCulture);
            Name = sector.Name;
            OneDayPriceChgPerCent = sector.OneDayPriceChgPerCent;
            MarketCap = sector.MarketCap;
            PriceToEarnings = sector.PriceToEarnings;
            ROEPerCent = sector.ROEPerCent;
            DivYieldPerCent = sector.DivYieldPerCent;
            DebtToEquity = sector.DebtToEquity;
            PriceToBook = sector.PriceToBook;
            NetProfitMarginMrq = sector.NetProfitMarginMrq;
            PriceToFreeCashFlowMrq = sector.PriceToFreeCashFlowMrq;
        }

        internal Sector ConvertToSector()
        {
            return new Sector
            {
                Id = this.Id,
                Date = Convert.ToDateTime(this.WCFXferDate),
                Name = this.Name,
                OneDayPriceChgPerCent = this.OneDayPriceChgPerCent,
                MarketCap = this.MarketCap,
                PriceToEarnings = this.PriceToEarnings,
                ROEPerCent = this.ROEPerCent,
                DivYieldPerCent = this.DivYieldPerCent,
                DebtToEquity = this.DebtToEquity,
                PriceToBook = this.PriceToBook,
                NetProfitMarginMrq = this.NetProfitMarginMrq,
                PriceToFreeCashFlowMrq = this.PriceToFreeCashFlowMrq
            };
        }
    }
}
