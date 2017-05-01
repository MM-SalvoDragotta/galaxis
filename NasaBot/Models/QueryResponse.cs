using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NasaBot.Models
{
    public class QueryResponse
    {
        public string Response { get; set; }

        public string SessionId { get; set; }

        public string Intent { get; set; }
    }
}