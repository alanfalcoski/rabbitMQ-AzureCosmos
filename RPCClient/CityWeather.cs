namespace RPCClient
{
    public class CityWeather
    {                
        public string City { get; set; }
        public string Summary { get; set; }
        public decimal TemperatureC { get; set; }
        public DateTime Date { get; set; }

        public CityWeather(DateTime date, Decimal tempC, string summary, string city)
        {
            this.City = city;            
            this.TemperatureC = tempC;
            this.Summary = summary;
            this.Date = date;
        }
    }
}
