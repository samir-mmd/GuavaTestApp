using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuavaStart.Services
{
    public class UserAuthMessenger
    {
        public IConnection connection;
        public IModel channel;
        public string replyQueueName;
        public string correlationId;
        public EventingBasicConsumer consumer;
        public IBasicProperties props;

        public UserAuthMessenger(string message)
        {
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            props = channel.CreateBasicProperties();
            correlationId = Guid.NewGuid().ToString();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            channel.BasicConsume(
                consumer: consumer,
                queue: replyQueueName,
                autoAck: false);

           var responceBytes = Encoding.UTF8.GetBytes(message);
           channel.BasicPublish(
                exchange: "",
                routingKey: "rpc_queue",
                basicProperties: props,
                body: responceBytes);
        }
    }
}
