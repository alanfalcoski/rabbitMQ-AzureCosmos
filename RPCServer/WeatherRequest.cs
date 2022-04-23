using System.Text;
using System.Text.Json.Nodes;


namespace RPCServer
{
    public class WeatherRequest
    {
        #region "attibutes"
        private const string apikey = "eb8b1a9405e659b2ffc78f0a520b1a46";
        private const string host = "api.openweathermap.org";
        private string uri = string.Empty;
        #endregion
        public CityWeather GetCityWeather(string city)
        {
            uri = $"https://{host}/data/2.5/weather?q={city}&appid={apikey}&units=metric";

            var client = new HttpClient();

            var webRequest = new HttpRequestMessage(HttpMethod.Get, uri)
            {
                Content = new StringContent("{}", Encoding.UTF8, "application/json")
            };

            var response = client.Send(webRequest);

            using var reader = new StreamReader(response.Content.ReadAsStream());

            string result = reader.ReadToEnd();
                                               
            if (!string.IsNullOrEmpty(result)) 
            {
                JsonObject? jsonObj = JsonNode.Parse(result).AsObject();
                return new CityWeather(Convert.ToString(jsonObj["dt"]),
                                        Convert.ToString(jsonObj["main"]["temp"]),
                                        Convert.ToString(jsonObj["weather"][0]["description"]),
                                        Convert.ToString(city));
            } else 
            {
                throw new Exception("Objeto de terceiro inválido.");
            }
        }
    }
}
