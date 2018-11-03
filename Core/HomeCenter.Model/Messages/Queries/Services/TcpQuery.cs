using HomeCenter.Model.Messages.Queries;
using System.Collections.Generic;
using System.Net;

namespace HomeCenter.Model.Messages.Commands.Service
{

    public class TcpQuery : Query
    {
        public string Address { get; set; }
        public byte[] Body { get; set; }
    }
}