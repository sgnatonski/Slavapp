using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using SlavApp.Microservice.Microservice;

namespace SlavApp.Microservice.Monitor
{
    public class MicroserviceMonitor
    {
        public event EventHandler<ServiceStatusMessage> OnStatusChanged;
        private QueueClient client;
        public void Initialize(string queueConnString, string queueName)
        {
            try
            {
                var messagingFactory = MessagingFactory.CreateFromConnectionString(queueConnString);
                client = messagingFactory.CreateQueueClient(queueName, ReceiveMode.ReceiveAndDelete);

                var options = new OnMessageOptions {AutoComplete = false, AutoRenewTimeout = TimeSpan.FromMinutes(1)};

                // Callback to handle received messages.
                client.OnMessage(msg =>
                {
                    var stream = msg.GetBody<Stream>();
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        var json = Encoding.UTF8.GetString(ms.ToArray());
                        var status = JsonConvert.DeserializeObject<ServiceStatusMessage>(json);
                        if (OnStatusChanged != null)
                        {
                            OnStatusChanged(this, status);
                        }
                    }
                }, options);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
