using System;

namespace PackageInstaller.Editor
{
    [Serializable]
    internal struct PackageRecord
    {
        public string PackageName;
        public string PackageId;
        public string PackageUrl;

        public string[] Dependencies;
    }
    
    [Serializable]
    internal struct PackageRecordArray
    {
        public PackageGroupRecord[] Groups;
    }

    [Serializable]
    internal struct PackageGroupRecord
    {
        public string Name;
        public PackageRecord[] Records;
    }
}