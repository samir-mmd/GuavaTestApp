using GuavaStart.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GuavaStart.Messages
{
    public class UserAuthMessage
    {
        public string head { get; set; }
        public string result { get; set; }
        public GuavaUser guavaUser { get; set; }

    }
}
