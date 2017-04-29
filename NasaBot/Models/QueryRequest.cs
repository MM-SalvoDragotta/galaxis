using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NasaBot.Models
{
    public class QueryRequest
    {
        public string Query { get; set; }

        public string SessionId { get; set; }
    }
}