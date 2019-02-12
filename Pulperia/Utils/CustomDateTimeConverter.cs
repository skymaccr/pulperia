using Newtonsoft.Json.Converters;

namespace Pulperia.Utils
{
    public class CustomDateTimeConverter : IsoDateTimeConverter
    {
        public CustomDateTimeConverter()
        {
            base.DateTimeFormat = "MM/dd/yyyy";
        }
    }
}