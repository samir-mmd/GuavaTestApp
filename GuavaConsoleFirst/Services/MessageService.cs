using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaConsoleFirst.Services
{
    public class MessageService
    {
        ConnectionFactory _factory;
        public IConnection _conn;
        public IModel _channel;
        public MessageService()
        {
            Console.WriteLine("about to connect to rabbit");

            _factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672 };
            _factory.UserName = "guest";
            _factory.Password = "guest";
            _conn = _factory.CreateConnection();
            _channel = _conn.CreateModel();
            //_channel.QueueDeclare(queue: "rpc_queue",
            //                        durable: true,
            //                        exclusive: false,
            //                        autoDelete: false,
            //                        arguments: null);
            _channel.QueueDeclare(queue: "rpc_queue",
                                  durable: true,
                                  exclusive: false,
                                  autoDelete: false,
                                  arguments: null);
            _channel.BasicQos(0, 1, false);
        }

        public IModel GetChannel()
        {
            return _channel;
        }


        public bool EnqueueStraight(string messageString, string key)
        {
            var body = Encoding.UTF8.GetBytes("server processed " + messageString);

            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            _channel.BasicPublish(exchange: "",
                                routingKey: key,
                                basicProperties: properties,
                                body: body);
            Console.WriteLine(" [x] Published {0} to with key {1} RabbitMQ", messageString, key);
            return true;
        }
    }
}
