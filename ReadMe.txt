Preparation:
Download tons of stuff from https://www.microsoft.com/en-us/download/confirmation.aspx?id=51657
Update nuget packages in the sln
Verify - Build solution :-)

Settings:
1. create a (classic)cloud service via Azure dashboard
2. create a storage account
MAKE SURE THEY BELONG TO THE SAME "Resource group"
3. get the "Connection string" for the storage account: 
	in Azure portal, go to the create storage account. On the "Setting" section, open "Access keys".
	Copy the value from "CONNECTION STRING".
4. Copy the Connection string to \NectarCloud\ServiceConfiguration.Cloud.cscfg, as the value of :
	a. "ConnectionString"
	b. "Microsoft.WindowsAzure.Plugins.Diagnostics.ConnectionString"
5. Copy the Connection string to \NECTAR\App.config, as the value of "ConnectionString"
6. When deploying make sure you set the correct cloud service.

Config run:
1. NectarConfig - place your config in \nectarworkerrole\app.config, under the <!-- Put configs here -->. Make them using CreateConfigQuery.linq.
2. numOfGraphsPerConfig - change the value of the numOfGraphsPerConfig variable in \NECTAR\Program.cs
3. numOfInstances - TWO PLACES TO CHANGE!!! In \NectarCloud\ServiceConfiguration.Cloud.cscfg, change 
	a. Instances count
	b. Value of Setting name="numOfInstances"
	VALUES MUST MATCH!
4. tablename - change the value of the talbename variable in \NECTAR\Program.cs - BEWARE(!) make sure you follow the rules for a table name:https://blogs.msdn.microsoft.com/jmstall/2014/06/12/azure-storage-naming-rules/
   Table names must be unique within an account.
	Table names may contain only alphanumeric characters.
	Table names cannot begin with a numeric character.
	Table names are case-insensitive.
	Table names must be from 3 to 63 characters long.
	Some table names are reserved, including "tables". Attempting to create a table with a reserved table name returns error code 404 (Bad Request).
	These rules are also described by the regular expression "^[A-Za-z][A-Za-z0-9]{2,62}$".
5. KeepInstancesAliveWhenDone - In \NectarCloud\ServiceConfiguration.Cloud.cscfg, change the value of Setting name="KeepInstancesAliveWhenDone""


NON MANDATORY config
5. Enable RDP on machines

Start
NectarCloud -> right click -> publish


Tools
1. To watch the data created - azure data explorer: https://azure.microsoft.com/en-us/features/storage-explorer/
	(Use the name and key found in the connection string to connect to your data)
2. startup log location - connect to the via (via Azure portal). Go to:
	C:\Resources\Directory\<instanceID>.NectarWorkerRole.LogsPath
3. To watch the logs - go to table "WADLogsTable". Filter partitionKey >= "0636438503486739869"
	which is "0" + (DateTime.UtcNow.AddMinutes(-60).Ticks)

	TODO:
	Change to webrole, so it will die once done.
	if you want it to die after completion you should change the offering
	to webjobs
	or a scheduler
	which suppose to be easy since all of the logic is inside the NECTAR.Program
	 https://www.hanselman.com/blog/IntroducingWindowsAzureWebJobs.aspx
	https://sandervandevelde.wordpress.com/2016/07/19/turning-a-console-app-into-a-long-running-azure-webjob/
	to sum up - you can move the logic that is currently in RunAsync (WorkerRole.cs) to your NECTAR.Program.Main
	and the publish it as a WebJob with schedule for 1 instance or 10 instances
	
	https://docs.microsoft.com/en-us/azure/azure-functions/functions-create-first-azure-function 
another option
can wait for trigger 
you can launch http request with the configuation 
it will calculate and quit