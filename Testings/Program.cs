namespace Testings
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    using Models;
    using System.Threading.Tasks.Dataflow;

    public class Program
    {
        private static string tableName = "todo";

        public static void Main(string[] args)
        {
            UpdateCheckpoints().Wait();
        }

        public static async Task UpdateCheckpoints()
        {
            var connString = ConfigurationManager.AppSettings["ConnectionString"];
            var nectarClient = new NectarStorageClient(connString, tableName.ToLower());
            var lastCheckpoint = DateTime.SpecifyKind(DateTime.MinValue, DateTimeKind.Utc);
            var validateBlock = new TransformBlock<LfrEntity, Tuple<LfrEntity, bool>>(
                        str => ValidateBlob(str, nectarClient),
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 1 });
            var updateCheckpoint = new ActionBlock<Tuple<LfrEntity, bool>>(
                res => PerformUpdate(res, nectarClient),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 1 });
            var runIds = await nectarClient.GetAllRunIds(lastCheckpoint);
            validateBlock.LinkTo(updateCheckpoint, new DataflowLinkOptions { PropagateCompletion = true });

            var tasks = runIds.Select(id => validateBlock.SendAsync(id));
            await Task.WhenAll(tasks);
            validateBlock.Complete();
            await updateCheckpoint.Completion;
        }

        public static async Task PerformUpdate(Tuple<LfrEntity, bool> result, NectarStorageClient client)
        {
            if (!result.Item2)
            {
                await client.DeleteEntity(result.Item1);
                return;
            }

            await client.UpdateCheckpoint(result.Item1.ConfigElement);
        }

        public static async Task ExecuteAsync()
        {
            var connString = ConfigurationManager.AppSettings["ConnectionString"];
            var nectarClient = new NectarStorageClient(connString, tableName.ToLower());
            var lastCheckpoint = DateTime.SpecifyKind(DateTime.Parse("2017-07-19T07:41:58.045Z").AddHours(-3), DateTimeKind.Utc);
            using (var writer = new StreamWriter(@"C:\Nectar\validateBlob.txt"))
            {
                while (true)
                {
                    var validateBlock = new ActionBlock<LfrEntity>(
                        str => ValidateBlob(str, nectarClient, writer),
                        new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 1, BoundedCapacity = 1 });
                    var runIds = await nectarClient.GetAllRunIds(lastCheckpoint);
                    var tasks = runIds.Select(id => validateBlock.SendAsync(id));
                    await Task.WhenAll(tasks);
                    validateBlock.Complete();
                    await validateBlock.Completion;

                    lastCheckpoint = DateTime.SpecifyKind(runIds.Max(a => a.Timestamp.DateTime), DateTimeKind.Utc);

                    await writer.WriteLineAsync($"[{DateTime.UtcNow}]: Finished validating {runIds.Count} blobs, last checkpoint: {lastCheckpoint}");

                    await writer.FlushAsync();
                    await Task.Delay(TimeSpan.FromMinutes(15));
                }
            }
        }

        public static async Task<Tuple<LfrEntity, bool>> ValidateBlob(LfrEntity runId, NectarStorageClient client, StreamWriter writer = null)
        {
            var lfrEnt = await client.GetLfrGraphFromTable(runId.RowKey);
            var graph = lfrEnt.Graph;
            if (graph.Vertices.Count != 1000 || graph.Vertices.Any(kvp => !kvp.Value.Any()))
            {
                if (writer != null)
                {
                    await writer.WriteLineAsync($"***** Bad graph found {runId}");
                }

                return new Tuple<LfrEntity, bool>(runId, false);
            }

            return new Tuple<LfrEntity, bool>(runId, true);
        }
    }
}
