using System;

namespace PackageInstaller.Editor
{
    [Serializable]
    internal struct PackageRecord
    {
        public string PackageName;
        public string PackageId;
        public string PackageUrl;
    }
    
    [Serializable]
    internal struct PackageRecordArray
    {
        public PackageRecord[] Records;
    }
}