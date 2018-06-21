<Query Kind="Statements" />

var dict = @"AvgDegree=10;20;40
NumOfMemberships=2;3;4,2;3;4;5;6,2;3;4;5;6;7;8
MaxDegree=50,50,50;100
MixingParam=0.1;0.2;0.3,0.1;0.2;0.3;0.4,0.1;0.2;0.3;0.4;0.5
CommSizeDistExp=1
MinCommSize=5;20,30;50
MaxCommSize=20;50,50;100
NumOfOverlappingNodes=2000;5000;10000".Split('\n').ToDictionary(str => str.Split('=')[0], str => str.Split('=')[1]);
var i = -1;
var j = 0;
var name = 0;
foreach (var val in dict["AvgDegree"].Split(';')) 
{
	i++;
	foreach (var member in dict["NumOfMemberships"].Split(',')[i].Split(';')) 
	{
		foreach (var maxDegree in dict["MaxDegree"].Split(',')[i].Split(';')) 
		{
			foreach (var mixingParam in dict["MixingParam"].Split(',')[i].Split(';')) 			
			{
				foreach (var CommSizeDistExp in dict["CommSizeDistExp"].Split(';'))  
					{
				//foreach (var minComm in dict["MinCommSize"].Split(',')[i].Split(';'))  
				//{
					//var maxComm = dict["MaxCommSize"].Split(',')[i].Split(';')[j];
					//foreach (var maxComm in dict["MaxCommSize"].Split(',')[i].Split(';')) 
					//{
						foreach (var overlapping in dict["NumOfOverlappingNodes"].Split(';'))  
						{
							string.Format(
@"<RunConfig Name=""{7}"">
<Lfr
	NumberOfNodes=""20000""
	AvgDegree=""{0}""
	NumOfMemberships=""{1}""
	MaxDegree=""{2}""
	DegreeSeqExpo=""2""
	MixingParam=""{3}""
	CommSizeDistExp=""{8}""
	MinCommSize=""{4}""
	MaxCommSize=""{5}""
	NumOfOverlappingNodes=""{6}"" />
<Nectar
	Betas=""1.01;1.05;1.09;1.1;1.2;1.3;1.4"" 
	Alpha=""0.8""
	StartMergeAt=""4""
	MaxNumberOfIterations=""20""
	InitMode=""0""
	PrecentageOfStableNodes=""95"" />
</RunConfig>
", val, member, maxDegree, mixingParam, 1, 1, overlapping, name++, CommSizeDistExp).Dump();
							
						}
					//}
					j++;
				}
				j = 0;
			}
		}
	}	
}