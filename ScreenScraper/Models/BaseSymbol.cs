using System.Runtime.Serialization;

namespace ScreenScraper.Models
{
    internal class BaseSymbol
    {
        protected string dateForSerialization;

        [OnDeserializing]
        private void OnDeserializing(StreamingContext context)
        {
            this.dateForSerialization = "1900-01-01";
        }
    }
}
