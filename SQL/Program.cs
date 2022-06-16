using System;
using System.Threading.Tasks;
using System.Configuration;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Cosmos;

namespace ForecastRepository
{
    public class Cosmos
    {
        #region "Attributes"        
        private static readonly string EndpointUri = "";
        private static readonly string PrimaryKey = "";
        private CosmosClient cosmosClient;        
        public Database database;        
        public Container container;        
        private string databaseId = "amcom";
        private string containerId = "forecasts";

        #endregion
        #region "Database init"
        public static async Task Main(string[] args)
        {                            
                Cosmos p = new Cosmos();
                await p.StartDB();
        }

        /// <summary>
        /// Checks if a database exists.
        /// </summary>
        /// <param name="databaseName">Name of the database to check.</param>
        /// <returns>True, if the database exists, otherwise false.</returns>
        public async Task<bool> DatabaseExistsAsync(string databaseName)
        {
            var databaseNames = new List<string>();
            using (FeedIterator<DatabaseProperties> iterator = cosmosClient.GetDatabaseQueryIterator<DatabaseProperties>())
            {
                while (iterator.HasMoreResults)
                {
                    foreach (DatabaseProperties databaseProperties in await iterator.ReadNextAsync())
                    {
                        databaseNames.Add(databaseProperties.Id);
                    }
                }
            }

            return databaseNames.Contains(databaseName);
        }
        
        /// <summary>
        /// Create AMCOM database/container on cosmos 
        /// </summary>
        public async Task StartDB()
        {            
            this.cosmosClient = new CosmosClient(EndpointUri, PrimaryKey, new CosmosClientOptions() { ApplicationName = "WeatherForecastCity" });
            if ((!await this.DatabaseExistsAsync("amcom")))
            {
                await this.CreateDatabaseAsync();
                await this.CreateContainerAsync();
            }            
            this.container = cosmosClient.GetContainer("amcom", "forecasts");
            this.database = cosmosClient.GetDatabase("amcom");
        }
        

        // <CreateDatabaseAsync>
        /// <summary>
        /// Create the database if it does not exist
        /// </summary>
        private async Task CreateDatabaseAsync()
        {            
            this.database = await this.cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            Console.WriteLine("Created Database: {0}\n", this.database.Id);
        }        
        
        /// <summary>
        /// Create the container if it does not exist. 
        /// Specifiy "/partitionKey" as the partition key path since we're storing family information, to ensure good distribution of requests and storage.
        /// </summary>        
        private async Task CreateContainerAsync()
        {        
            this.container = await this.database.CreateContainerIfNotExistsAsync(containerId, "/partitionKey");
            Console.WriteLine("Created Container: {0}\n", this.container.Id);
        }

        #endregion
        #region "Database CRUD"        
        /// <summary>
        /// Add forecast items to the container
        /// </summary>       
        public async Task AddItemsToContainerAsync(Forecast forecast)
        {
            try
            {                  
                ItemResponse<Forecast> forecastResponse = await this.container.ReadItemAsync<Forecast>(forecast.Id, new PartitionKey(forecast.PartitionKey));
                Console.WriteLine("Item in database with id: {0} already exists\n", forecastResponse.Resource.Id);
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                ItemResponse<Forecast> forecastResponse = await this.container.CreateItemAsync<Forecast>(forecast, new PartitionKey(forecast.PartitionKey));
                Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", forecastResponse.Resource.Id, forecastResponse.RequestCharge);
            }
        }
    }
    #endregion
}
