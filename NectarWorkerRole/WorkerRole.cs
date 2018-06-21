using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace NectarWorkerRole
{
    using System.Configuration;

    using Microsoft.Azure;

    using Models;

    using NECTAR;

    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        public override void Run()
        {
            Trace.TraceInformation("NectarWorkerRole is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            catch (Exception e)
            {
                Trace.TraceError($"ERROR: NectarWorkerRole.RunAsync throw exception  {e}");
            }
            finally
            {
                Trace.TraceInformation("NectarWorkerRole in finally");
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            Trace.TraceInformation("NectarWorkerRole is OnStart");
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            bool result = base.OnStart();

            Trace.TraceInformation("NectarWorkerRole has been started");

            return result;
        }

        public override void OnStop()
        {
            Trace.TraceInformation("NectarWorkerRole is stopping");

            this.cancellationTokenSource.Cancel();
            this.runCompleteEvent.WaitOne();

            base.OnStop();

            Trace.TraceInformation("NectarWorkerRole has stopped");
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            Trace.TraceInformation("NectarWorkerRole in RunAsync");
            var connString = CloudConfigurationManager.GetSetting("ConnectionString");
            var tableName = NECTAR.Program.tableName.ToLower();
            var storageClient = new NectarStorageClient(connString, tableName);
            var confSection = ConfigurationManager.GetSection(RunConfigSection.SectionName) as RunConfigSection;
            var idAsString = RoleEnvironment.CurrentRoleInstance.Id;
            var instanceId = int.Parse(idAsString.Substring(idAsString.LastIndexOf("_") + 1));
            // Change as you want... Should match value in ServiceConfiguration.Cloud.cscfg
            var numOfInstances = Convert.ToInt32(CloudConfigurationManager.GetSetting("numOfInstances")); 
            var confElements = confSection.RunConfigs.Cast<RunConfigElement>().
                Where(confElement => int.Parse(confElement.Name) % numOfInstances == instanceId);
            try
            {
                await Program.ExecuteWithBlocksAsync(confElements, storageClient, numOfGraphsPerConfig: NECTAR.Program.numOfGraphsPerConfig);
            }
            catch(Exception e)
            {
                Trace.TraceError($"ERROR: NectarWorkerRole.ExecuteWithBlocksAsync throw exception  {e}");
            }

            Trace.TraceInformation($"DONE: NectarWorkerRole done with RunAsync");

            while (Convert.ToBoolean(CloudConfigurationManager.GetSetting("KeepInstancesAliveWhenDone")))
            {
                await Task.Delay(TimeSpan.FromHours(1));
            }
            // TODO -ask Herman if there should be a OnStop() here.
        }
    }
}
