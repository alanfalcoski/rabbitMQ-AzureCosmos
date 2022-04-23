using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace ForecastRepository
{
    public class Forecast
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }
        public string City { get; set; }
        public string Summary { get; set; }
        public decimal TemperatureC { get; set; }
        public DateTime Date { get; set; }
                       
        public Forecast(DateTime date, decimal TemperatureC, string Summary, string city)
        {
            this.City = city;            
            this.TemperatureC = TemperatureC;
            this.Summary = Summary;
            this.Date = date;
            this.Id = Guid.NewGuid().ToString();
            this.PartitionKey = "amcom";            
        }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }        
    }
}
