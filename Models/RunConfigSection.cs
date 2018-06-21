namespace Models
{
    using System;
    using System.Configuration;

    using Microsoft.WindowsAzure.Storage.Blob.Protocol;

    using Newtonsoft.Json;

    public class RunConfigSection : ConfigurationSection
    {
        public const string SectionName = "NectarConfig";

        [ConfigurationProperty(nameof(RunConfigs), IsRequired = true)]
        [ConfigurationCollection(typeof(RunConfigsElementCollection), AddItemName = "RunConfig")]
        public RunConfigsElementCollection RunConfigs => (RunConfigsElementCollection)base[nameof(RunConfigs)];
    }

    public class RunConfigsElementCollection : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new RunConfigElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((RunConfigElement)element).Name;
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class RunConfigElement : ConfigurationElement
    {
        [JsonProperty]
        [ConfigurationProperty("Lfr", IsRequired = true)]
        public LfrConfigElement Lfr
        {
            get
            {
                return (LfrConfigElement)base["Lfr"];
            }
            set
            {
                base["Lfr"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("Nectar", IsRequired = true)]
        public NectarConfigElement Nectar
        {
            get
            {
                return (NectarConfigElement)base["Nectar"];
            }
            set
            {
                base["Nectar"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("Name", IsRequired = true)]
        public string Name
        {
            get
            {
                return (string)base["Name"];
            }
            set
            {
                base["Name"] = value;
            }
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class NectarConfigElement : ConfigurationElement
    {
        [JsonProperty]
        [ConfigurationProperty("Betas", IsRequired = true)]
        public string Betas
        {
            get
            {
                return (string)base["Betas"];
            }
            set
            {
                base["Betas"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("Alpha", IsRequired = false, DefaultValue = 0.8)]
        public double Alpha
        {
            get
            {
                return (double)base["Alpha"];
            }
            set
            {
                if (value > 1 || value < 0)
                {
                    throw new ArgumentException("Alpha must be between 0 and 1");
                }

                base["Alpha"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("StartMergeAt", IsRequired = false, DefaultValue = 6)]
        public int StartMergeAt
        {
            get
            {
                return (int)base["StartMergeAt"];
            }
            set
            {
                base["StartMergeAt"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("MaxNumberOfIterations", IsRequired = false, DefaultValue = 20)]
        public int MaxNumberOfIterations
        {
            get
            {
                return (int)base["MaxNumberOfIterations"];
            }
            set
            {
                base["MaxNumberOfIterations"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("InitMode", IsRequired = false, DefaultValue = 0)]
        public int InitMode
        {
            get
            {
                return (int)base["InitMode"];
            }
            set
            {
                if (value != 0 && value != 3 && value != 4)
                {
                    throw new ArgumentException("Wrong InitMode number - must be either 0,3,4");
                }

                base["InitMode"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("PrecentageOfStableNodes", IsRequired = false, DefaultValue = 95)]
        public int PercentageOfStableNodes
        {
            get
            {
                return (int)base["PrecentageOfStableNodes"];
            }
            set
            {
                if (value >= 100 || value <= 0)
                {
                    throw new ArgumentException("Precentage must be between 0 and 100");
                }

                base["PrecentageOfStableNodes"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("DynamicChoose", IsRequired = false, DefaultValue = false)]
        public bool DynamicChoose => false;

        [JsonProperty]
        [ConfigurationProperty("UseWOCC", IsRequired = false, DefaultValue = false)]
        public bool UseWOCC
        {
            get
            {
                return (bool)base["UseWOCC"];
            }
            set
            {
                base["UseWOCC"] = value;
            }
        }

        public string GetCommandLineArguments()
        {
            return 
                $"{Betas.Replace(';', ',')} {Alpha} {StartMergeAt} {MaxNumberOfIterations} {InitMode} {PercentageOfStableNodes} {DynamicChoose}";
        }
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class LfrConfigElement : ConfigurationElement
    {
        [JsonProperty]
        [ConfigurationProperty("NumberOfNodes", IsRequired = true)]
        public string NumberOfNodes
        {
            get
            {
                return (string)base["NumberOfNodes"];
            }
            set
            {
                base["NumberOfNodes"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("AvgDegree", IsRequired = true)]
        public string AvgDegree
        {
            get
            {
                return (string)base["AvgDegree"];
            }
            set
            {
                base["AvgDegree"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("MaxDegree", IsRequired = true)]
        public string MaxDegree
        {
            get
            {
                return (string)base["MaxDegree"];
            }
            set
            {
                base["MaxDegree"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("MixingParam", IsRequired = true)]
        public string MixingParam
        {
            get
            {
                return (string)base["MixingParam"];
            }
            set
            {
                base["MixingParam"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("DegreeSeqExpo", IsRequired = true)]
        public string DegreeSeqExpo
        {
            get
            {
                return (string)base["DegreeSeqExpo"];
            }
            set
            {
                base["DegreeSeqExpo"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("CommSizeDistExp", IsRequired = true)]
        public string CommSizeDistExp
        {
            get
            {
                return (string)base["CommSizeDistExp"];
            }
            set
            {
                base["CommSizeDistExp"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("MinCommSize", IsRequired = true)]
        public string MinCommSize
        {
            get
            {
                return (string)base["MinCommSize"];
            }
            set
            {
                base["MinCommSize"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("MaxCommSize", IsRequired = true)]
        public string MaxCommSize
        {
            get
            {
                return (string)base["MaxCommSize"];
            }
            set
            {
                base["MaxCommSize"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("NumOfOverlappingNodes", IsRequired = true)]
        public string NumOfOverlappingNodes
        {
            get
            {
                return (string)base["NumOfOverlappingNodes"];
            }
            set
            {
                base["NumOfOverlappingNodes"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("NumOfMemberships", IsRequired = true)]
        public string NumOfMemberships
        {
            get
            {
                return (string)base["NumOfMemberships"];
            }
            set
            {
                base["NumOfMemberships"] = value;
            }
        }

        [JsonProperty]
        [ConfigurationProperty("AvgClusteringCoeff", IsRequired = false)]
        public string AvgClusteringCoeff
        {
            get
            {
                return (string)base["AvgClusteringCoeff"];
            }
            set
            {
                base["AvgClusteringCoeff"] = value;
            }
        }

        public string GetCommandLineArguments()
        {
            // Letting LFR decide comm sizes
            return
                $"-N {NumberOfNodes} -k {AvgDegree} -maxk {MaxDegree} -mu {MixingParam} -t1 {DegreeSeqExpo} -t2 {CommSizeDistExp} "
                + $"-on {NumOfOverlappingNodes} -om {NumOfMemberships}";
            /*return
                $"-N {NumberOfNodes} -k {AvgDegree} -maxk {MaxDegree} -mu {MixingParam} -t1 {DegreeSeqExpo} -t2 {CommSizeDistExp} "
                + $"-minc {MinCommSize} -maxc {MaxCommSize} -on {NumOfOverlappingNodes} -om {NumOfMemberships}";
                */
        }
    }
}
