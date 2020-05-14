using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;

namespace IndexInfo.WebApi
{
    public class AzureServiceBus : IServiceBus
    {
        const string ServiceBusConnectionString = "Endpoint=sb://etfarb.servicebus.windows.net/;SharedAccessKeyName=sendlisten;SharedAccessKey=SyNoMMSoWe9sjHbypPs37fg9ayHsz00L5aZaV43gV98=";
        const string QueueName = "stockpricequeue";
        static IQueueClient _queueClient;

        public AzureServiceBus()
        {
            _queueClient = new QueueClient(ServiceBusConnectionString, QueueName);
        }

        public Task ProcessMessageAsync(params object[] args)
        {
            throw new NotImplementedException();
        }

        public void RegisterOnMessageHandlerAndReceiveMessages(params object[] args)
        {
            throw new NotImplementedException();
        }

        public async Task SendCommandAsync(ICommand command)
        {
            await SendMessagesAsync((Message)command.Message);
        }

        private async Task SendMessagesAsync(Message message)
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
    }
}