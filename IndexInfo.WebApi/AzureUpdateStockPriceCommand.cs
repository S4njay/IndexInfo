using System.Collections.Generic;
using System.Text;
using Microsoft.Azure.ServiceBus;
using Newtonsoft.Json;

namespace IndexInfo.WebApi
{
    public class AzureUpdateStockPriceCommand : IUpdateStockPriceCommand
    {
        private string id;

        public AzureUpdateStockPriceCommand(string id)
        {
            this.id = id;
        }

        public string QueueName {get;set;}
        public object Message
        {
            get
            {
                var body = new Dictionary<string, object>
            {
                {"command", "updateStockPriceCommand"},
                {"args", new { id = id}}
            };
                return new Message(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body)));
            }
            private set
            {

            }
        }
    }
}