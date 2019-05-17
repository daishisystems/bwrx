using System.Collections.Generic;
using System.Web.Http;
using Bwrx.Api;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        [Monitor("flight-search")]
        public IEnumerable<string> Get()
        {
            var count = Blacklist.Instance.IpAddresses.Count;
            return new[] {"value1", "value2"};
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}