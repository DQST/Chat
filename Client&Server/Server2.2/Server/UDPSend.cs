using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Server
{
    public interface UDPSend
    {
        void Send(string msg, string ip, int port);
        void Recieve();
    }
}
