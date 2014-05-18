using System.Collections.Generic;
using System.Data;
using ScreenScraper.Models;

namespace ScreenScraper.BulkLoad
{
    public class BulkLoadSymbols : BaseBulkLoad
    {
        private static readonly string[] ColumnNames = new string[]
            {
                "Date", "ExchangeName", "HasAnalyst", "HasData",
                "HasEstimates", "HasHolders", "HasInsider", "HasKeyStats", "HasOptions", "HasSummary",
                "Industry", "IndustryId", "Name",
                "Sector", "SectorId", "Symbol"
            };

        public BulkLoadSymbols()
            : base(ColumnNames)
        {

        }

        public DataTable LoadDataTableWithIndustries(IEnumerable<SymbolDetail> dStats, DataTable dt)
        {
            foreach (var value in dStats)
            {
                var sValue = value.Date + "^" + value.ExchangeName + "^" + value.HasAnalyst + "^"
                             + value.HasData + "^" + value.HasEstimates
                             + "^" + value.HasHolders + "^" + value.HasInsider + "^" + value.HasKeyStats
                             + "^" + value.HasOptions + "^" + value.HasSummary + "^" +
                             value.Industry + "^" + value.IndustryId + "^" + value.Name
                             + "^" + value.Sector + "^" + value.SectorId + "^" + value.Symbol;

                DataRow row = dt.NewRow();

                row.ItemArray = sValue.Split('^');

                dt.Rows.Add(row);
            }

            return dt;
        }
    }
}
