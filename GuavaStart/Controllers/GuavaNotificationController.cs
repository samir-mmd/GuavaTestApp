using GuavaStart.Globals;
using GuavaStart.Models;
using GuavaStart.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using GuavaStart.Messages;
using System.Net.WebSockets;
using System.Net;
using System.Threading;
using System.IO;
using Microsoft.AspNetCore.Authorization;

namespace GuavaStart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    
    public class GuavaNotificationController : ControllerBase
    {
       
        public GuavaNotificationController()
        {           
           
        }


        /// <summary>
        /// Web Socket connection point. Marks connected and autorized user to recieve notifications from stockservice
        /// </summary>
        [HttpGet("ws")]
        public async Task Get()
        {
            Console.WriteLine("WS Opened");
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using WebSocket webSocket = await
                                   HttpContext.WebSockets.AcceptWebSocketAsync();
                await Echo(HttpContext, webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            }
        }

        private async Task Echo(HttpContext context, WebSocket webSocket)
        {
            GlobalVars.WsUserId = GlobalVars.CurrentUserId;
            var factory = new ConnectionFactory() { HostName = "rabbitmq" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "notificationqueue",
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (model, ea) =>
                {
                    Console.WriteLine("Message Recieved from: notificationqueue");
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine(" NOTIFICATION Received {0}", message);
                    
                    var request = Encoding.UTF8.GetString(ea.Body.ToArray(),
                                            0,
                                            ea.Body.ToArray().Length);
                    var type = WebSocketMessageType.Text;
                    var data = Encoding.UTF8.GetBytes("Echo from server :" + request);                   
                    webSocket.SendAsync(ea.Body.ToArray(), type, true, CancellationToken.None);

                    channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                    //webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);
                };
                channel.BasicConsume(queue: "notificationqueue",
                                     autoAck: false,
                                     consumer: consumer);
                Console.WriteLine("Listening to queue: notificationqueue");
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                Console.WriteLine("WS data recieved");
                while (!result.CloseStatus.HasValue)
                {
                    
                }
                channel.QueueDeleteNoWait("notificationqueue");
                await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
            GlobalVars.WsUserId = -1;
            Console.WriteLine("DISCONNECTED");
        }


        /// <summary>
        /// Using by stockservice
        /// </summary>
        [HttpGet]
        [Route("getwsuserID")]
        public int GetWsUserID()
        {
            return GlobalVars.WsUserId;
        }
    }
}
