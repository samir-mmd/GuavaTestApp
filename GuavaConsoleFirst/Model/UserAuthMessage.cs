using System;
using System.Collections.Generic;
using System.Text;

namespace GuavaConsoleFirst.Model
{
    public class UserAuthMessage
    {
        public string head { get; set; }
        public string result { get; set; }
        public GuavaUser guavaUser { get; set; }
    }
}
