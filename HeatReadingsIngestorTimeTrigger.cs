using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DataPipeline.Functions.Models;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using ServerlessDataPipeline.Configuration;

namespace ServerlessDataPipeline
{
    public static class HeatReadingsIngestorTimeTrigger
    {
        public static Guid DemoClient = new Guid("3b930dcf-3cef-4964-9ebd-408f4c53ce8b");

        [FunctionName("HeatReadingsIngestorTimeTrigger")]
        public static async Task RunAsync(
            [TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, 
            ILogger log,
            [CosmosDB(
                databaseName: Constants.CosmosDbDatabaseName,
                collectionName: Constants.CosmosDbCollectionName,
                ConnectionStringSetting = Constants.CosmosHourlyDataReadWriteConnectionString)] DocumentClient client)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            await DoRun(client, log).ConfigureAwait(false);
        }

        private static async Task DoRun(DocumentClient documentClient, ILogger log)
        {
            var uri = UriFactory.CreateDocumentCollectionUri(
                        Constants.CosmosDbDatabaseName, Constants.CosmosDbCollectionName);

            var random = new Random();
            IEnumerable<Task> inserts = Enumerable.Range(0, 100)
                .Select(async (i) =>
                {
                    await documentClient.CreateDocumentAsync(uri, new HeatReading
                    {
                        ClientId = DemoClient,
                        TimeStamp = DateTimeOffset.UtcNow.Date.AddHours(-random.Next(0, 72)),
                        Value = 10 * random.NextDouble()
                    }).ConfigureAwait(false);
                });

            await Task.WhenAll(inserts).ConfigureAwait(false);
        }
    }
}
