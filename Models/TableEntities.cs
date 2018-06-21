namespace Models
{
    using System;
    using System.Diagnostics;
    using System.Security.Cryptography.X509Certificates;

    using Microsoft.WindowsAzure.Storage.Table;

    using Newtonsoft.Json;

    public class LfrEntity : TableEntity
    {
        public LfrEntity(string runId, RunConfigElement confElement, string graphUri) : base("lfr", runId)
        {
            ConfigElement = confElement;
            GraphUri = graphUri;
        }

        public LfrEntity()
        {
        }

        public string ConfigElementAsJson { get; set; }

        public string GraphUri { get; set; }

        [IgnoreProperty]
        public Graph Graph { get; set; }

        [IgnoreProperty]
        public RunConfigElement ConfigElement
        {
            get
            {
                return JsonConvert.DeserializeObject<RunConfigElement>(ConfigElementAsJson);
            }
            set
            {
                ConfigElementAsJson = JsonConvert.SerializeObject(value);
            }
        }
    }

    public class NectarEntity : TableEntity
    {
        public NectarEntity(string runId, string graphUri) : base("nectar", runId)
        {
            GraphUri = graphUri;
        }

        public NectarEntity()
        {
        }

        public string GraphUri { get; set; }

        [IgnoreProperty]
        public Graph Graph { get; set; }
    }

    public class MetricSumEntity : TableEntity
    {
        public MetricSumEntity(string runId) : base ("scores", runId)
        {
        }

        public MetricSumEntity()
        {
        }

        public string MetricsSumString { get; set; }

        [IgnoreProperty]
        public decimal MetricsSum
        {
            get
            {
                return Decimal.Parse(MetricsSumString);
            }
            set
            {
                MetricsSumString = value.ToString();
            }
        }

        public string OmegaIndexString { get; set; }

        [IgnoreProperty]
        public decimal OmegaIndex
        {
            get
            {
                return decimal.Parse(OmegaIndexString);
            }
            set
            {
                OmegaIndexString = value.ToString();
            }
        }

        public string AverageF1String { get; set; }

        [IgnoreProperty]
        public decimal AverageF1
        {
            get
            {
                return decimal.Parse(AverageF1String);
            }
            set
            {
                AverageF1String = value.ToString();
            }
        }

        public string OnmiString { get; set; }

        [IgnoreProperty]
        public decimal Onmi
        {
            get
            {
                return decimal.Parse(OnmiString);
            }
            set
            {
                OnmiString = value.ToString();
            }
        }
    }

    public class FeaturesEntity : TableEntity
    {
        public FeaturesEntity(string runId, string confName, string numOfNodesS, string configElementAsJson)
            : base("labels", runId)
        {
            ConfigName = confName;
            NumOfNodesInGraph = numOfNodesS;
            ConfigElementAsJson = configElementAsJson;
        }

        public FeaturesEntity()
        {
        }

        public string NumOfNodesInGraph { get; set; }
        public string ConfigName { get; set; }
        public string ConfigElementAsJson { get; set; }
        
        public double GCC { get; set; }
        public double ACC { get; set; }
        public double AverageDegree { get; set; }
        public double AvergaeTrianglesRate { get; set; }
        public double RatioOfNodesInTriangles { get; set; }

        //true = WOCC, false = !WOCC
        public string Label { get; set; }

        public string WOCCScoreString { get; set; }

        [IgnoreProperty]
        public decimal WOCCScore
        {
            get
            {
                return Decimal.Parse(WOCCScoreString);
            }
            set
            {
                WOCCScoreString = value.ToString();
            }
        }

        public string ModScoreString { get; set; }

        [IgnoreProperty]
        public decimal ModScore
        {
            get
            {
                return Decimal.Parse(ModScoreString);
            }
            set
            {
                ModScoreString = value.ToString();
            }
        }

        public string WeightStr { get; set; }

        [IgnoreProperty]
        public decimal Weight
        {
            get
            {
                return Decimal.Parse(WeightStr);
            }
            set
            {
                WeightStr = value.ToString();
            }
        }

        public string Beta { get; set; }

        [IgnoreProperty]
        public string BetaPrv
        {
            get
            {
                return Beta;
            }
            set
            {
                try
                {
                    Beta = value.Split('_')[1];
                }
                catch (Exception e)
                {
                    Trace.TraceError($"ERROR {e} - unable to set beta");
                    Beta = "0";
                }
            }
        }

        public void SetLabel()
        {
            Label = WOCCScore > ModScore ? "1" : "0";
        }

        public void SetWeight()
        {
            try
            {
                var trunModScore = Math.Truncate(ModScore * 1000m) / 1000m;
                var trunWOCCScore = Math.Truncate(WOCCScore * 1000m) / 1000m;
                var diff = Math.Abs(trunModScore - trunWOCCScore);
                var max = Math.Max(trunModScore, trunWOCCScore) <= 0 ? 0 : Math.Max(trunModScore, trunWOCCScore);
                Weight = max == 0 ? 0 : diff / max;
            }
            catch (Exception e)
            {
                Trace.TraceError($"ERROR in SetWeight {e} - setting weight to 0");
                Weight = 0;                
            }
        }

        public void SetLabelAndWeight()
        {
            SetLabel();
            SetWeight();
        }
    }

    public class CheckpointEntity : TableEntity
    {
        public CheckpointEntity(int configNumber, int nodeNumber) 
            : base("checkpoint", $"{nodeNumber}_{configNumber}")
        {
        }

        public CheckpointEntity()
        {
        }

        public int GraphCount { get; set; }
    }
}
