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
using System.Net.Http;

namespace GuavaStart.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GuavaUserController : ControllerBase
    {
       
        private BlockingCollection<GuavaUser> users = new BlockingCollection<GuavaUser>();

        public GuavaUserController()
        {           
           
        }


        /// <summary>
        /// Sends autorizations data to UserEngine and returns a token
        /// </summary>
        /// <response code="200">Returns a new token and a username</response>
        /// <response code="400">Username or password is wrong</response>
        /// <response code="204">Message Service unavailable</response>
        [HttpPost("autorize")]        
        public IActionResult Token(string username, string password)
        {
            var identity = GetIdentity(username, password);
            if (identity == null)
            {
                return new BadRequestResult();
            }
                                 
            var now = DateTime.UtcNow;
            
            var jwt = new JwtSecurityToken(
                    issuer: AuthOptions.ISSUER,
                    audience: AuthOptions.AUDIENCE,
                    notBefore: now,
                    claims: identity.Claims,
                    expires: now.Add(TimeSpan.FromMinutes(AuthOptions.LIFETIME)),
                    signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256));
          

            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var response = new
            {
                access_token = encodedJwt,
                username = identity.Name,               
            }; 
            return new JsonResult(response);
        }

        /// <summary>
        /// Sends Reg. info to UserEngine.
        /// </summary>
        /// <response code="200">Returns the newly created user with no token</response>
        /// <response code="400">User already exists</response>
        /// <response code="204">Message Service unavailable</response>
        [HttpPost("register")]
        public IActionResult Register(string username,string password)
        {            
            UserAuthMessage response = new UserAuthMessage();
            var message = JsonConvert.SerializeObject(new UserAuthMessage { head = "Register", guavaUser = new GuavaUser { userName = username, Password = password } }, Formatting.Indented);
            try
            {
                UserAuthMessenger userAuthMessenger = new UserAuthMessenger(message);

                userAuthMessenger.consumer.Received += (model, ea) =>
                {
                    if (ea.BasicProperties.CorrelationId == userAuthMessenger.correlationId)
                    {
                        var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                        response = JsonConvert.DeserializeObject<UserAuthMessage>(body);
                        users.Add(response.guavaUser);
                        userAuthMessenger.channel.BasicAck(deliveryTag: ea.DeliveryTag,
                          multiple: false);
                        userAuthMessenger.channel.QueueDeleteNoWait(userAuthMessenger.replyQueueName);
                    }
                    else
                    {
                        userAuthMessenger.channel.BasicReject(deliveryTag: ea.DeliveryTag, true);
                    }
                };
            }
            catch (Exception)
            {

                return new NoContentResult();
            }
            

            GuavaUser user = users.Take();
            Console.WriteLine(response.result);
            if (response.result == "Success")
            {
                return new JsonResult(user);
            }
            else
            {
                return new BadRequestResult();
            }          
        }

        private ClaimsIdentity GetIdentity(string username, string password)
        {
            GlobalVars.CurrentUserId = -1;
            UserAuthMessage response = new UserAuthMessage();
            var message = JsonConvert.SerializeObject(new UserAuthMessage { head = "Authtorize", guavaUser = new GuavaUser { userName = username, Password = password } }, Formatting.Indented);
            UserAuthMessenger userAuthMessenger = new UserAuthMessenger(message);

            userAuthMessenger.consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == userAuthMessenger.correlationId)
                {
                    var body = Encoding.UTF8.GetString(ea.Body.ToArray());
                    response = JsonConvert.DeserializeObject<UserAuthMessage>(body);
                    users.Add(response.guavaUser);
                    userAuthMessenger.channel.BasicAck(deliveryTag: ea.DeliveryTag,
                      multiple: false);
                    userAuthMessenger.channel.QueueDeleteNoWait(userAuthMessenger.replyQueueName);
                }
                else
                {
                    userAuthMessenger.channel.BasicReject(deliveryTag: ea.DeliveryTag, true);
                }
            };
            GuavaUser user = users.Take();
            Console.WriteLine(response.result);
            if (response.result == "Success")
            {
                if (user != null)
                {
                    var claims = new List<Claim>
                {
                    new Claim(ClaimsIdentity.DefaultNameClaimType, user.userName)
                };
                    ClaimsIdentity claimsIdentity =
                    new ClaimsIdentity(claims, "Token", ClaimsIdentity.DefaultNameClaimType,
                        ClaimsIdentity.DefaultRoleClaimType);
                    GlobalVars.CurrentUserId = user.Id;
                    GlobalVars.WsUserId = -1;
                    return claimsIdentity;
                }
            }            
            return null;

        }
    }
}
