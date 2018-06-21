namespace Convertors
{
    using System;
    using System.IO;

    using Microsoft.WindowsAzure.ServiceRuntime;

    public class TempFile : IDisposable
    {
        public readonly string Path;

        public bool DeleteAfterFinish;

        public static string LocalStoragePrefix
        {
            get
            {
                try
                {
                    return RoleEnvironment.IsAvailable ? $"{RoleEnvironment.GetLocalResource("Nectar").RootPath}\\" : @"C:\Nectar\";
                }
                catch (Exception)
                {
                    return @"C:\Nectar\";
                }
            }            
        }

        public TempFile(string path, bool isFullPath = false, bool deleteAfterFinish = true)
        {
            Path = isFullPath ? path : $"{LocalStoragePrefix}{path}";
            DeleteAfterFinish = deleteAfterFinish;
        }

        public void Dispose()
        {
            if (!string.IsNullOrWhiteSpace(Path) && File.Exists(Path) && DeleteAfterFinish)
            {
                File.Delete(Path);
            }
        }

        public override string ToString()
        {
            return Path;
        }
    }
}
