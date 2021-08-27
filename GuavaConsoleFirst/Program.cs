using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Newtonsoft.Json;
using GuavaConsoleFirst.Services;
using GuavaConsoleFirst.Model;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading;

namespace GuavaConsoleFirst
{
    public class Program
    {
        public static ConnectionFactory _factory;
        public static IConnection _conn;
        public static IModel _channel;        

        static void Main(string[] args)
        {            
            var consumer = new EventingBasicConsumer(_channel);
            while (true)
            {
                try
                {
                    Console.WriteLine("about to connect to rabbit");
                    _factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672 };
                    _factory.UserName = "guest";
                    _factory.Password = "guest";
                    _conn = _factory.CreateConnection();
                    _channel = _conn.CreateModel();
                    _channel.QueueDeclare(queue: "rpc_queue",
                                          durable: true,
                                          exclusive: false,
                                          autoDelete: false,
                                          arguments: null);
                    _channel.BasicQos(0, 1, false);
                    consumer.Received += (model, ea) =>
                    {
                        Console.WriteLine("MESSAGE RECIEVED");
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body.ToArray());
                        UserEngine(ea, _channel);
                    };

                    _channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);

                    break;
                }
                catch (Exception)
                {
                    Console.WriteLine("Unable to connect to RabbitMq, waiting 10 sec");
                    for (int i = 0; i < 11; i++)
                    {
                        Console.WriteLine(i);
                        Thread.Sleep(1000);
                    }
                }
            }
            
            while (true)
            {

            }
        }

        public async static void UserEngine(BasicDeliverEventArgs ea, IModel channel)
        {
            await Task.Run(()=>
            { 
                string response = null;
                            
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var userAuthMessage = JsonConvert.DeserializeObject<UserAuthMessage>(message);
                var props = ea.BasicProperties;
                var replyProps = channel.CreateBasicProperties();
                replyProps.CorrelationId = props.CorrelationId;

                try
                {

                    if (!String.IsNullOrWhiteSpace(userAuthMessage.guavaUser.userName) && !String.IsNullOrWhiteSpace(userAuthMessage.guavaUser.Password))
                    {
                        if (userAuthMessage.head == "Register")
                        {
                            RegisterUser(userAuthMessage);                            
                        }

                        if (userAuthMessage.head == "Authtorize")
                        {
                            AutorizeUser(userAuthMessage);
                        }
                    }
                    else
                    {
                        userAuthMessage.result = "Wrong Input";
                    }

                    response = JsonConvert.SerializeObject(userAuthMessage, Formatting.Indented);


                }
                catch (Exception e)
                {
                    Console.WriteLine(" UserEngine Exception on [UserRegister] " + e.Message);
                    response = "";
                }
                finally
                {
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    channel.BasicPublish(exchange: "", routingKey: props.ReplyTo,
                      basicProperties: replyProps, body: responseBytes);
                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                }
                Console.WriteLine("Resuming");
            } );
        }

        public static void RegisterUser(UserAuthMessage userAuthMessage)
        {
            using (UserDbContext db = new UserDbContext())
            {
                if (!db.GuavaUsers.Where(n => n.userName == userAuthMessage.guavaUser.userName).Any())
                {
                    Console.WriteLine($"Creating user {userAuthMessage.guavaUser.userName} with passworkd {userAuthMessage.guavaUser.Password}");
                    GuavaUser user = new GuavaUser { userName = userAuthMessage.guavaUser.userName, Password = userAuthMessage.guavaUser.Password };
                    db.GuavaUsers.Add(user);
                    db.SaveChanges();
                    var users = db.GuavaUsers.ToListAsync();

                    for (int i = 0; i < users.Result.Count; i++)
                    {
                        Console.WriteLine(users.Result[i].userName);
                    }
                    userAuthMessage.guavaUser = db.GuavaUsers.Where(x => x.userName == userAuthMessage.guavaUser.userName).First();
                    userAuthMessage.result = "Success";
                }
                else
                {
                    userAuthMessage.result = "User Already Exists";
                }
            }
        }

        public static void AutorizeUser(UserAuthMessage userAuthMessage)
        {
            using (UserDbContext db = new UserDbContext())
            {
                if (db.GuavaUsers.Where(n => n.userName == userAuthMessage.guavaUser.userName).Any())
                {
                    if (db.GuavaUsers.Where(n => n.userName == userAuthMessage.guavaUser.userName && n.Password == userAuthMessage.guavaUser.Password).Any())
                    {
                        userAuthMessage.guavaUser = db.GuavaUsers.Where(x => x.userName == userAuthMessage.guavaUser.userName).First();
                        userAuthMessage.result = "Success";
                    }
                    else
                    {
                        userAuthMessage.result = "Wrong Password";
                    }
                }
                else
                {
                    userAuthMessage.result = "User not found";
                }
            }
        }

    }
}


