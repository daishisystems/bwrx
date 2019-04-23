using System.Collections.Generic;
using System.Web.Http;
using Bwrx.Api;

namespace WebApplication1.Controllers
{
    public class AvailabilityController : ApiController
    {
        public IEnumerable<string> Get()
        {
            var count = Blacklist.Instance.IpAddresses.Count;
            return new[] {"value1", "value2"};
        }
    }
}