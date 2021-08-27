using GuavaStart.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GuavaStart.Globals
{
    public static class GlobalVars
    {
        static public int CurrentUserId;
        static public int WsUserId;
    }
}
