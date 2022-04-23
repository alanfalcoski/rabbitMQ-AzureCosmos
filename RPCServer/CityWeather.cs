using System.Globalization;

namespace RPCServer
{
    public class CityWeather
    {
        public string City { get; set; }
        public string? Summary { get; set; }
        public decimal TemperatureC { get; set; }       
        public DateTime Date { get; set; }

        public CityWeather(string date, string tempC, string summary, string city)
        {
            this.City = city;            
            this.TemperatureC = decimal.Parse(tempC, new NumberFormatInfo() { NumberDecimalSeparator = "." });
            this.Summary = summary;
            this.Date = DateTimeOffset.FromUnixTimeSeconds(long.Parse(date)).ToLocalTime().DateTime;
        }
    }
}
