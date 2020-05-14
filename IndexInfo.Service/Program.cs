using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Hosting;

namespace IndexInfo.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            var bus = new AzureServiceBus();
            var host = new HostBuilder().Build();
            host.Run();
        }
    }

    public class AzureServiceBus : IServiceBus
    {
        const string ServiceBusConnectionString = "Endpoint=sb://etfarb.servicebus.windows.net/;SharedAccessKeyName=sendlisten;SharedAccessKey=SyNoMMSoWe9sjHbypPs37fg9ayHsz00L5aZaV43gV98=";
        const string QueueName = "stockpricequeue";
        static QueueClient _queueClient;

        public AzureServiceBus()
        {
            _queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
            RegisterOnMessageHandlerAndReceiveMessages();
        }

        public async Task ProcessMessageAsync(params object[] args)
        {
            var message = (Message)args[0];
            // Process the message.
            var messageBody = Encoding.UTF8.GetString(message.Body);
            Console.WriteLine($"Received message: Body:{messageBody}");
            var messageBodyDict = JsonDocument.Parse(messageBody);

            switch (messageBodyDict.RootElement.GetProperty("command").GetString())
            {
                case "updateStockPriceCommand":
                    await UpdateStockPriceAsync(messageBodyDict
                    .RootElement
                    .GetProperty("args")
                    .GetProperty("id")
                    .GetString());
                    break;
            }

            // Complete the message so that it is not received again.
            // This can be done only if the queue Client is created in ReceiveMode.PeekLock mode (which is the default).
            await _queueClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the queueClient has already been closed.
            // If queueClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        private async Task UpdateStockPriceAsync(string id)
        {
            using (var client = new HttpClient())
            {
                // TODO: move to config
                var streamAsync =  client.GetStringAsync(
                    $"http://localhost:5000/api/v1/quotes?id={id}").Result;
                                
                await client.PutAsync($"http://localhost:5001/api/StockPrices/{id}",
                new StringContent(streamAsync, Encoding.UTF8, "application/json"));
            }
        }

        public void RegisterOnMessageHandlerAndReceiveMessages(params object[] args)
        {
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether the message pump should automatically complete the messages after returning from user callback.
                // False below indicates the complete operation is handled by the user callback as in ProcessMessagesAsync().
                AutoComplete = false
            };

            // Register the function that processes messages.
            _queueClient.RegisterMessageHandler(ProcessMessages, messageHandlerOptions);
        }

        private async Task ProcessMessages(Message arg1, CancellationToken arg2)
        {
            await ProcessMessageAsync(new object[] { (object)arg1, (object)arg2 }).ConfigureAwait(false);
        }

        public async Task SendCommandAsync(ICommand command)
        {
            await SendMessagesAsync(command.QueueName, (Message)command.Message);
        }

        private async Task SendMessagesAsync(string QueueName, Message message)
        {
            try
            {
                // Write the body of the message to the console.
                Console.WriteLine($"Sending message: {message.Body}");

                // Send the message to the queue.
                await _queueClient.SendAsync(message);

            }
            catch (Exception exception)
            {
                Console.WriteLine($"{DateTime.Now} :: Exception: {exception.Message}");
            }
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }
    }

    public class YahooStockInfo
    {
        public double? SandP52WeekChange { get; set; }
        public string address1 { get; set; }
        public double? ask { get; set; }
        public long? askSize { get; set; }
        public long? averageDailyVolume10Day { get; set; }
        public long? averageVolume { get; set; }
        public long? averageVolume10days { get; set; }
        public double? beta { get; set; }
        public double? bid { get; set; }
        public long? bidSize { get; set; }
        public double? bookValue { get; set; }
        public string city { get; set; }
        public string country { get; set; }
        public string currency { get; set; }
        public long? dateShortInterest { get; set; }
        public double? dayHigh { get; set; }
        public double? dayLow { get; set; }
        public double? dividendRate { get; set; }
        public double? dividendYield { get; set; }
        public double? earningsQuarterlyGrowth { get; set; }
        public double? enterpriseToEbitda { get; set; }
        public double? enterpriseToRevenue { get; set; }
        public long? enterpriseValue { get; set; }
        public long? exDividendDate { get; set; }
        public string exchange { get; set; }
        public string exchangeTimezoneName { get; set; }
        public string exchangeTimezoneShortName { get; set; }
        public string fax { get; set; }
        public double? fiftyDayAverage { get; set; }
        public double? fiftyTwoWeekHigh { get; set; }
        public double? fiftyTwoWeekLow { get; set; }
        public double? fiveYearAvgDividendYield { get; set; }
        public long? floatShares { get; set; }
        public double? forwardEps { get; set; }
        public double? forwardPE { get; set; }
        public long? fullTimeEmployees { get; set; }
        public string gmtOffSetMilliseconds { get; set; }
        public double? heldPercentInsiders { get; set; }
        public double? heldPercentInstitutions { get; set; }
        public string industry { get; set; }
        public bool isEsgPopulated { get; set; }
        public long? lastFiscalYearEnd { get; set; }
        public long? lastSplitDate { get; set; }
        public string lastSplitFactor { get; set; }
        public string logo_url { get; set; }
        public string longBusinessSummary { get; set; }
        public string longName { get; set; }
        public string market { get; set; }
        public long? marketCap { get; set; }
        public long? maxAge { get; set; }
        public string messageBoardId { get; set; }
        public long? mostRecentQuarter { get; set; }
        public long? netIncomeToCommon { get; set; }
        public long? nextFiscalYearEnd { get; set; }
        public double? open { get; set; }
        public double? payoutRatio { get; set; }
        public double? pegRatio { get; set; }
        public string phone { get; set; }
        public double? previousClose { get; set; }
        public long? priceHint { get; set; }
        public double? priceToBook { get; set; }
        public double? priceToSalesTrailing12Months { get; set; }
        public double? profitMargins { get; set; }
        public string quoteType { get; set; }
        public double? regularMarketDayHigh { get; set; }
        public double? regularMarketDayLow { get; set; }
        public double? regularMarketOpen { get; set; }
        public double? regularMarketPreviousClose { get; set; }
        public double? regularMarketPrice { get; set; }
        public long? regularMarketVolume { get; set; }
        public string sector { get; set; }
        public long? sharesOutstanding { get; set; }
        public double? sharesPercentSharesOut { get; set; }
        public long? sharesShort { get; set; }
        public long? sharesShortPreviousMonthDate { get; set; }
        public long? sharesShortPriorMonth { get; set; }
        public string shortName { get; set; }
        public double? shortPercentOfFloat { get; set; }
        public double? shortRatio { get; set; }
        public string state { get; set; }
        public string symbol { get; set; }
        public bool tradeable { get; set; }
        public double? trailingAnnualDividendRate { get; set; }
        public double? trailingAnnualDividendYield { get; set; }
        public double? trailingEps { get; set; }
        public double? trailingPE { get; set; }
        public double? twoHundredDayAverage { get; set; }
        public long? volume { get; set; }
        public string website { get; set; }
        public string zip { get; set; }

    }

    public class StockPrice : YahooStockInfo
    {
        public new string symbol {get;set;}
        public DateTime dateUpdated {get;set;}  
    }
}
