using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using Bwrx.Api;
using Newtonsoft.Json;

namespace WebApplication1.Controllers
{
    public class ValuesController : ApiController
    {
        [Monitor("flight-search")]
        public async Task<IEnumerable<string>> Get()
        {
            var httpContent = await Request.Content.ReadAsStringAsync();
            var count = Blacklist.Instance.IpAddresses.Count;
            return new[] {"value1", "value2"};
        }

        // GET api/values/5
        public string Get(int id)
        {
            switch (id)
            {
                case 0:
                    Agent.Instance.Shutdown();
                    return "shutdown";
                case 1:
                {
                    var credentials = JsonConvert.DeserializeObject<CloudServiceCredentials>(Resources.Credentials);
                    var config = JsonConvert.DeserializeObject<ClientConfigSettings>(Resources.ClientConfigSettings);
                    Agent.Instance.Start(credentials, config);
                    return "start";
                }
                default:
                    EventMetaCache.Instance.Add(new {Name = "Pablo "}, "event");
                    return "add";
            }
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