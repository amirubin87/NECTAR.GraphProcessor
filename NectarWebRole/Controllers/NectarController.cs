namespace NectarWebRole.Controllers
{
    using System;
    using System.Configuration;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Models;
    using Microsoft.Azure;

    using Newtonsoft.Json;

    using NECTAR;

    public class NectarController : ApiController
    {
        public async Task<IHttpActionResult> Post([FromBody] RunConfigElement conf)
        {
            var connString = CloudConfigurationManager.GetSetting("ConnectionString");
            var storageClient = new NectarStorageClient(connString);
            try
            {
                await Program.ExecuteAsync(conf, storageClient);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

            return Ok();
        }

        public Task<IHttpActionResult> Get()
        {
            var connString = CloudConfigurationManager.GetSetting("ConnectionString");
            var storageClient = new NectarStorageClient(connString);
            var confSection = ConfigurationManager.GetSection(RunConfigSection.SectionName) as RunConfigSection;

            var tasksList = (from RunConfigElement confElement in confSection.RunConfigs select Program.ExecuteAsync(confElement, storageClient)).ToList();
            Task.WhenAll(tasksList);
            return Task.FromResult<IHttpActionResult>(Ok());
        }
    }
}