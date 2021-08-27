using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaStockService.Services
{
    public class NotificationMessenger
    {
        public IConnection connection;
        public IModel channel;
        public string replyQueueName;
        public string correlationId;
        public EventingBasicConsumer consumer;
        public IBasicProperties props;

        public NotificationMessenger(string message)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672 };
            factory.UserName = "guest";
            factory.Password = "guest";
            connection = factory.CreateConnection();
            channel = connection.CreateModel(); 
            props = channel.CreateBasicProperties(); 
            var responceBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(
                 exchange: "",
                 routingKey: "notificationqueue",
                 basicProperties: props,
                 body: responceBytes);
            Console.WriteLine("Sending notification message to notificationqueue");
        }
    }
}
