using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using RPCServer;
using System.Text.Json;

#region "Server Listening RabbitMQ request"
var factory = new ConnectionFactory() { HostName = "localhost" };
using var connection = factory.CreateConnection();
using var channel = connection.CreateModel();

channel.QueueDeclare(queue: "forecast_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);
channel.BasicQos(0, 1, false);
var consumer = new EventingBasicConsumer(channel);
channel.BasicConsume(queue: "forecast_queue", autoAck: false, consumer: consumer);
Console.WriteLine(" [x] Awaiting cities weather requests");

consumer.Received += (model, ea) =>
{
    string response = string.Empty;

    var body = ea.Body.ToArray();
    var props = ea.BasicProperties;
    var replyProps = channel.CreateBasicProperties();
    replyProps.CorrelationId = props.CorrelationId;

    try
    {
        var message = Encoding.UTF8.GetString(body);
        string n = message;
        Console.WriteLine($" [.] city({message})");
        response = GetWeather(n).ToString();
    }
    catch (Exception e)
    {
        Console.WriteLine($" [.] {e.Message}");
        response = "";
    }
    finally
    {
        var responseBytes = Encoding.UTF8.GetBytes(response);
        channel.BasicPublish(exchange: "", routingKey: props.ReplyTo, basicProperties: replyProps, body: responseBytes);
        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
    }
};

Console.WriteLine(" Press [enter] to exit.");
Console.ReadLine();
#endregion

/// <summary>
/// Will call openweather api and get wheater of a specific city.
/// </summary>
static string GetWeather(string city)
{
    WeatherRequest weatherRequest = new WeatherRequest();
    return JsonSerializer.Serialize(weatherRequest.GetCityWeather(city));
}