using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using CsvHelper;
using CsvHelper.Configuration;
using Shared;
using System.Globalization;
using TriggerSender;

class Program
{
    static string queueName = @"exampleQueueName";
    static string connectionString = @"Endpoint=exampleValue";
    static ServiceBusClient client;
    static ServiceBusSender sender;
    static QueueRuntimeProperties queue;

    static int skipCount = 2000;
    static int takeCount = 500;
    const string csvPath = @"path_to/file.csv";

    static async Task Main()
    {
        client = new ServiceBusClient(connectionString);
        sender = client.CreateSender(queueName);

        // create a batch
        using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

        int currentBatchLength = 0;
        foreach (var item in GetSpList().Skip(skipCount).Take(takeCount))
        {
            Console.WriteLine(item);
            // try adding a message to the batch
            if (!messageBatch.TryAddMessage(new ServiceBusMessage(new TriggerMessage().getMessageObjCubeReady(item))))
            {
                // if it is too large for the batch
                throw new Exception($"The message {item} is too large to fit in the batch.");
            }
            currentBatchLength++;
        }

        try
        {
            await sender.SendMessagesAsync(messageBatch);
            Console.WriteLine($"A batch of messages has been published to the queue.");
        }
        finally
        {
            await sender.DisposeAsync();
            await client.DisposeAsync();
        }
    }

    static List<string> GetSpList()
    {
        var spList = new List<string>();
        var config = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
        };
        using (var reader = new StreamReader(csvPath))
        using (var csv = new CsvReader(reader, config))
        {
            var records = csv.GetRecords<StartPoint>();
            foreach (var item in records)
            {
                spList.Add(item.Name);
            }
        }
        return spList;
    }
}