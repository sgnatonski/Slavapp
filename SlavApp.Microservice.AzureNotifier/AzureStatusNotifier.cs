using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using SlavApp.Microservice.Microservice;

namespace SlavApp.Microservice.AzureNotifier
{
    public class AzureStatusNotifier : IMicroserviceStatusNotifier
    {
        private static QueueClient _client;
        private bool _isInitialized;
        private readonly Queue<string> _messagesQueue = new Queue<string>(); 
        public void Initialize(string queueConnString, string queueName)
        {
            Task.Run(() =>
            {
                if (_client != null) return;
                lock (this)
                {
                    if (_client != null) return;
                    try
                    {
                        var messagingFactory = MessagingFactory.CreateFromConnectionString(queueConnString);
                        _client = messagingFactory.CreateQueueClient(queueName, ReceiveMode.ReceiveAndDelete);
                        _isInitialized = true;
                        lock (this)
                        {
                            while (_messagesQueue.Any())
                            {
                                this.SendNotification(_messagesQueue.Dequeue());
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        if (_client != null)
                            _client.Close();
                        throw;
                    }
                }
            });
        }

        public void Notify(IMicroservice service, MicroserviceStatus status)
        {
            var json = JsonConvert.SerializeObject(new ServiceStatusMessage()
            {
                DateOn = DateTimeOffset.Now,
                Identity = service.Identifier,
                Status = status
            });

            if (!_isInitialized)
            {
                _messagesQueue.Enqueue(json);
            }
            else
            {
                SendNotification(json);
            }
        }

        private void SendNotification(string json)
        {
            if (string.IsNullOrEmpty(json))
                return;
            var msg = new BrokeredMessage(new MemoryStream(Encoding.UTF8.GetBytes(json)), true);
            _client.Send(msg);
        }
    }
}
