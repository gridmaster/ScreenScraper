using System;
using Newtonsoft.Json;

namespace ScreenScraper
{
    public class IndustryView
    {
        [JsonProperty(PropertyName = "Id")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "Sector")]
        public string Sector { get; set; }
        [JsonProperty(PropertyName = "Date")]
        public DateTime Date { get; set; }
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

    }
}
