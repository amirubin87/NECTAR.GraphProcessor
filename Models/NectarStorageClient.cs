namespace Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Blob;
    using Microsoft.WindowsAzure.Storage.Table;

    using Newtonsoft.Json;

    public class NectarStorageClient
    {
        private readonly CloudBlobContainer m_blobContainer;
        private readonly CloudTable m_table;
        private readonly CloudTableClient m_client;

        private const string c_checkpointPartitionName = "checkpoint";

        public NectarStorageClient(string connString, string input)
        {
            var storageAccount = CloudStorageAccount.Parse(connString);
            var blobClient = storageAccount.CreateCloudBlobClient();
            m_blobContainer = blobClient.GetContainerReference(input);
            m_client = storageAccount.CreateCloudTableClient();
            m_table = m_client.GetTableReference(input);
        }

        public async Task InsertGraphToTable(Graph graph, string runId, string partitionId = "lfr", RunConfigElement conf = null)
        {
            await m_table.CreateIfNotExistsAsync();
            await m_blobContainer.CreateIfNotExistsAsync();
            var blob = m_blobContainer.GetBlockBlobReference(runId);
            var graphAsJson = JsonConvert.SerializeObject(graph);
            using (var memStream = new MemoryStream(Encoding.Default.GetBytes(graphAsJson), false))
            {
                await blob.UploadFromStreamAsync(memStream);
            }

            switch (partitionId)
            {
                case "lfr":
                    var tableEnt = new LfrEntity(runId, conf, blob.Uri.ToString());
                    var tableOp = TableOperation.Insert(tableEnt);
                    await m_table.ExecuteAsync(tableOp);
                    break;
                case "nectar":
                    var nectarTableEnt = new NectarEntity(runId, blob.Uri.ToString());
                    var nectarTableOp = TableOperation.Insert(nectarTableEnt);
                    await m_table.ExecuteAsync(nectarTableOp);
                    break;
            }
        }

        public async Task<NectarEntity> GetNectarGraphFromTable(string runId, string beta, bool useWocc)
        {
            await m_table.CreateIfNotExistsAsync();
            var useWoccString = useWocc ? "1" : "0";
            var rowKey = $"{runId}_{beta}_{useWoccString}";
            var tableOp = TableOperation.Retrieve<NectarEntity>("nectar", rowKey);
            var opResult = await m_table.ExecuteAsync(tableOp);
            var nectarEnt = opResult.Result as NectarEntity;
            Graph graph = await GetGraphFromBlob(rowKey);

            nectarEnt.Graph = graph;
            return nectarEnt;
        }

        public async Task<LfrEntity> GetLfrGraphFromTable(string runId)
        {
            await m_table.CreateIfNotExistsAsync();
            var tableOp = TableOperation.Retrieve<LfrEntity>("lfr", runId);
            var opResult = await m_table.ExecuteAsync(tableOp);
            var lfrEnt = (opResult.Result) as LfrEntity;
            Graph graph = await GetGraphFromBlob(runId);

            lfrEnt.Graph = graph;
            return lfrEnt;
        }

        private async Task<Graph> GetGraphFromBlob(string runId)
        {
            var blob = await m_blobContainer.GetBlobReferenceFromServerAsync(runId);
            Graph graph;

            using (var memStream = new MemoryStream())
            using (var reader = new StreamReader(memStream))
            {
                await blob.DownloadToStreamAsync(memStream);
                memStream.Position = 0;
                var graphAsJson = await reader.ReadToEndAsync();
                graph = JsonConvert.DeserializeObject<Graph>(graphAsJson);
            }

            return graph;
        }

        public async Task<MetricSumEntity> InsertMetricToTable(decimal omegaIndexVal, decimal averageF1Val, decimal onmiValue, string runId)
        {
            await m_table.CreateIfNotExistsAsync();
            var sum = new MetricSumEntity(runId)
            {
                AverageF1 = averageF1Val,
                OmegaIndex = omegaIndexVal,
                Onmi = onmiValue,
                MetricsSum = averageF1Val + omegaIndexVal + onmiValue
            };

            var tableOp = TableOperation.Insert(sum);
            await m_table.ExecuteAsync(tableOp);

            return sum;
        }

        public async Task<IList<string>> GetAllRunIds()
        {
            await m_table.CreateIfNotExistsAsync();
            var cond = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "lfr");
            var query = new TableQuery<LfrEntity>().Select(new [] { "RowKey" }).Where(cond);
            var results = new List<string>();

            TableContinuationToken token = null;
            do
            {
                var queryRes = await m_table.ExecuteQuerySegmentedAsync(query, token);
                token = queryRes.ContinuationToken;
                results.AddRange(queryRes.Results.Select(ent => ent.RowKey));
            }
            while (token != null);

            return results;
        }

        public async Task<IList<LfrEntity>> GetAllRunIds(DateTime startTime)
        {
            await m_table.CreateIfNotExistsAsync();
            var cond = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, "lfr");
            if (startTime != default(DateTime))
            {
                var timeFilter = TableQuery.GenerateFilterConditionForDate("Timestamp", QueryComparisons.GreaterThan, startTime.ToUniversalTime());
                cond = $"{cond} and {timeFilter}";
            }

            var query = new TableQuery<LfrEntity>().Select(new[] { "RowKey", "Timestamp", nameof(LfrEntity.ConfigElementAsJson) }).Where(cond);
            var results = new List<LfrEntity>();

            TableContinuationToken token = null;
            do
            {
                var queryRes = await m_table.ExecuteQuerySegmentedAsync(query, token);
                token = queryRes.ContinuationToken;
                results.AddRange(queryRes.Results);
            }
            while (token != null);

            return results;
        }

        public async Task<IEnumerable<MetricSumEntity>> GetScoresByRunId(string runId, string[] betas, string useWocc)
        {
            await m_table.CreateIfNotExistsAsync();
            var rowNames = betas.Select(beta => $"'{runId}_{beta}_{useWocc}_metrics'").ToList();
            var cond = $"PartitionKey eq 'scores' and (RowKey eq {rowNames[0]} {string.Join(" ", rowNames.Skip(1).Select(name => $"or RowKey eq {name}"))})";
            var query = new TableQuery<MetricSumEntity>().Select(new[] { "MetricsSumString", "RowKey" }).Where(cond);
            var results = new List<MetricSumEntity>();

            TableContinuationToken token = null;
            do
            {
                var queryRes = await m_table.ExecuteQuerySegmentedAsync(query, token);
                token = queryRes.ContinuationToken;
                results.AddRange(queryRes.Results);
            }
            while (token != null);

            return results;
        }

        public async Task InsertFeaturesToTable(FeaturesEntity featureEntity, string outputtable = null)
        {
            if (string.IsNullOrWhiteSpace(outputtable))
            {
                await m_table.CreateIfNotExistsAsync();
                var tableOp = TableOperation.Insert(featureEntity);
                await m_table.ExecuteAsync(tableOp);
            }
            else
            {
                var table = m_client.GetTableReference(outputtable);
                await table.CreateIfNotExistsAsync();
                var tableOp = TableOperation.Insert(featureEntity);
                await table.ExecuteAsync(tableOp);
            }
        }

        public async Task<bool> ValidateConfig(RunConfigElement runConfigElement, int numOfGraphsPerConfig)
        {
            var rowKey = $"{runConfigElement.Lfr.NumberOfNodes}_{runConfigElement.Name}";
            await m_table.CreateIfNotExistsAsync();
            var tableOp = TableOperation.Retrieve<CheckpointEntity>(c_checkpointPartitionName, rowKey);
            var res = await m_table.ExecuteAsync(tableOp);
            var checkpointEnt = res.Result as CheckpointEntity;
            return checkpointEnt?.GraphCount >= numOfGraphsPerConfig;
        }

        public async Task UpdateCheckpoint(RunConfigElement confElement)
        {
            var rowKey = $"{confElement.Lfr.NumberOfNodes}_{confElement.Name}";
            await m_table.CreateIfNotExistsAsync();
            var tableOp = TableOperation.Retrieve<CheckpointEntity>(c_checkpointPartitionName, rowKey);
            var res = await m_table.ExecuteAsync(tableOp);
            var checkpointEnt = res.Result as CheckpointEntity;
            TableOperation insertOp;
            if (checkpointEnt == null)
            {
                checkpointEnt = new CheckpointEntity(int.Parse(confElement.Name), int.Parse(confElement.Lfr.NumberOfNodes)) { GraphCount = 1 };
                insertOp = TableOperation.Insert(checkpointEnt);
            }
            else
            {
                checkpointEnt.GraphCount += 1;
                insertOp = TableOperation.InsertOrMerge(checkpointEnt);
            }

            await m_table.ExecuteAsync(insertOp);
        }

        public async Task DeleteEntity(LfrEntity item)
        {
            await m_table.CreateIfNotExistsAsync();
            var tableop = TableOperation.Delete(item);
            await m_table.ExecuteAsync(tableop);
        }
    }
}
