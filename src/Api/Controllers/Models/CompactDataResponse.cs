using System.Collections.Generic;
using Microsoft.Identity.Client;

namespace Api.Controllers.Models
{
    public class CompactDataResponse
    {
        public List<string> Fields { get; set; } = new ();
        public List<object> Data { get; set; } = new ();
    }
}
