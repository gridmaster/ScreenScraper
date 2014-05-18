using System;

namespace ScreenScraper.Models
{
    public class SymbolDetail
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public DateTime Date { get; set; }
        public string Name { get; set; }

        public int SectorId { get; set; }
        public string Sector { get; set; }
        public int IndustryId { get; set; }
        public string Industry { get; set; }
        public int ExchangeId { get; set; }
        public string ExchangeName { get; set; }
        
        public bool HasSummary { get; set; }
        public bool HasData { get; set; }
        public bool HasOptions { get; set; }
        public bool HasInsider { get; set; }
        public bool HasAnalyst { get; set; }
        public bool HasEstimates { get; set; }
        public bool HasHolders { get; set; }
        public bool HasKeyStats { get; set; }
    }
}
    //[Id] [int] IDENTITY(1,1) NOT NULL,
    //[Symbol] [nvarchar](40) NOT NULL,
    //[Name] [nvarchar](200) NULL,
    //[ExchangeId] [nvarchar](20) NULL,
    //[ExchangeName] [nvarchar](40) NULL,
    //[SectorId] [int],
    //[Sector] [nvarchar](200) NULL,
    //[IndustryId] [int],
    //[Industry] [nvarchar](200) NULL,
    //[Date] [datetime] NULL,
    //[Timestamp] [datetime] NOT NULL,
    //[HasSummary] [bit] NULL,
    //[HasData] [bit] NULL,
    //[HasOptions] [bit] NULL,
    //[HasInsider] [bit] NULL,
    //[HasAnalyst] [bit] NULL,
    //[HasEstimates] [bit] NULL,
    //[HasHolders] [bit] NULL,
    //[HasKeyStats] [bit] NULL,