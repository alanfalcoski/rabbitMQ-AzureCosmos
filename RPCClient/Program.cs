using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Collections.Concurrent;
using System.Text;
using System.Net;
using ForecastRepository;
using RPCClient;
using Newtonsoft.Json;

#region RabbitMQ control
public class RpcClient
{
    #region "Attributes"
    private const string QUEUE_NAME = "forecast_queue";
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly ConcurrentDictionary<string, TaskCompletionSource<string>> callbackMapper = new();
    #endregion

    #region "Client listening for responses"
    public RpcClient()
    {
        //RabbbitMQ must be executing on default port.
        var factory = new ConnectionFactory() { HostName = "localhost" };

        connection = factory.CreateConnection();
        channel = connection.CreateModel();        
        replyQueueName = channel.QueueDeclare(queue: "").QueueName;
        consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            if (!callbackMapper.TryRemove(ea.BasicProperties.CorrelationId, out TaskCompletionSource<string>? tcs))
                return;
            var body = ea.Body.ToArray();
            var response = Encoding.UTF8.GetString(body);
            tcs.TrySetResult(response);
        };

        channel.BasicConsume(
          consumer: consumer,
          queue: replyQueueName,
          autoAck: true);
    }
    #endregion
    #region "Client send forecast request
    public Task<string> CallAsync(string message, CancellationToken cancellationToken = default)
    {
        IBasicProperties props = channel.CreateBasicProperties();
        var correlationId = Guid.NewGuid().ToString();
        props.CorrelationId = correlationId;
        props.ReplyTo = replyQueueName;
        var messageBytes = Encoding.UTF8.GetBytes(message);
        var tcs = new TaskCompletionSource<string>();
        callbackMapper.TryAdd(correlationId, tcs);

        channel.BasicPublish(
            exchange: "",
            routingKey: QUEUE_NAME,
            basicProperties: props,
            body: messageBytes);

        cancellationToken.Register(() => callbackMapper.TryRemove(correlationId, out var tmp));
        return tcs.Task;
    }
    #endregion   
}
#endregion
#region "Listening endpoint to execute requests"
public class Rpc
{
    public static void Main(string[] args)
    {
        using (var listener = new HttpListener())
        {
            listener.Prefixes.Add("http://localhost:8081/api/WeatherForecastCity/");
            listener.Start();
            for (; ; )
            {
                Console.WriteLine("Listening API...");
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                string key = request.QueryString.GetValues(0)[0];
                Task t = InvokeAsync(key);
                t.Wait();
                if (t.Id != 0)
                {
                    t.Dispose();
                    HttpListenerResponse response = context.Response;                    
                    string responseString = $"Chamada enviada para fila RabbitMQ com o id {t.Id} ";
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseString);                   
                    response.ContentLength64 = buffer.Length;
                    System.IO.Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);                    
                    output.Close();                    
                }
            }
        }                
    }

    public static async Task InvokeAsync(string n)
    {
        var rpcClient = new RpcClient();        
        var response = await rpcClient.CallAsync(n);
        
        await SendToRepository(await DeserializeForecast(response));
        Console.WriteLine(" [.] Got '{0}'", response);                
    }
    public static async Task<Cosmos> InstanceDatabase()
    {
        return new Cosmos();
    }
    public static async Task SendToRepository(Forecast forecastRepo)
    {
        Cosmos bancoCosmos = await InstanceDatabase();
        await bancoCosmos.StartDB();
        await bancoCosmos.AddItemsToContainerAsync(forecastRepo);
    }
    public static async Task<Forecast> DeserializeForecast(string response)
    {
        var obj = JsonConvert.DeserializeObject<CityWeather>(response);
        return new Forecast(obj.Date, obj.TemperatureC, obj.Summary, obj.City);
    }
    #endregion
}