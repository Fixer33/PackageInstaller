using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace PackageInstaller.Editor
{
    /// <summary>
    /// API for working with the Package Manager
    /// </summary>
    internal static class PackageInstaller
    {
        private static readonly PackageRecordArray _recordArray;

        static PackageInstaller()
        {
            string filePath = "Packages/com.fixer33.package-installer/packages-list.json";
#if PACKAGES_DEV
            filePath = "Assets/" + filePath;            
#endif
            var file = AssetDatabase.LoadAssetAtPath<TextAsset>(filePath);
            
            _recordArray = JsonUtility.FromJson<PackageRecordArray>(file.text);
        }

        #region Package listing

        private static ListRequest _listRequest;
        private static Action<List<string>> _listRequestCallback;

        internal static bool RefreshInstalledPackages(Action<List<string>> callback)
        {
            if (_listRequest is { IsCompleted: false })
                return false;

            _listRequestCallback = callback;
            _listRequest = Client.List();
            EditorApplication.update -= ListRequestProgress;
            EditorApplication.update += ListRequestProgress;
            return true;
        }

        private static void ListRequestProgress()
        {
            if (_listRequest.IsCompleted == false)
                return;
            
            EditorApplication.update -= ListRequestProgress;
            _listRequestCallback?.Invoke(_listRequest.Result.Select(i => i.packageId).ToList());
            _listRequestCallback = null;
            _listRequest = null;
        }

        #endregion

        #region Package adding

        private static AddAndRemoveRequest _activeAddRequest;
        private static Action _addRequestCallback;

        internal static void InstallPackages(PackageRecord[] packages, Action callback)
        {
            if (_activeAddRequest is { IsCompleted: false } )
                return;

            List<string> packagesToAdd = GetPackagesWithDependencies(packages);
            foreach (var p in packagesToAdd)
            {
                Debug.Log(p);
            }
            
            _addRequestCallback = callback;
            _activeAddRequest = Client.AddAndRemove(packagesToAdd
                .Select(i => GetPackageById(i).GetValueOrDefault(new PackageRecord(){PackageUrl = i}).PackageUrl).ToArray(), Array.Empty<string>());
            EditorApplication.update -= InstallPackageProgress;
            EditorApplication.update += InstallPackageProgress;
        }

        private static void InstallPackageProgress()
        {
            if (_activeAddRequest is { IsCompleted: false })
                return;
            
            EditorApplication.update -= InstallPackageProgress;
            _addRequestCallback?.Invoke();
            _addRequestCallback = null;
            _activeAddRequest = null;
        }

        #endregion

        internal static List<string> GetPackagesWithDependencies(PackageRecord[] packages)
        {
            List<PackageRecord> packageList = new(packages);
            List<string> result = new();
            bool hasNewDependencies = true;
            int cycles = 0;
            while (hasNewDependencies)
            {
                if (cycles++ >= 1000)
                {
                    throw new OverflowException("Dependency fetch cycle overloop! Aborting package install");
                }

                hasNewDependencies = false;
                foreach (var packageRecord in packageList.ToArray())
                {
                    if (result.Contains(packageRecord.PackageId) == false)
                        result.Add(packageRecord.PackageId);

                    if (packageRecord.Dependencies is not { Length: > 0})
                        continue;
                        
                    foreach (var packageDependency in packageRecord.Dependencies)
                    {
                        var packageById = GetPackageById(packageDependency);
                        if (packageById.HasValue)
                        {
                            if (packageList.Contains(packageById.Value) == false)
                            {
                                hasNewDependencies = true;
                                packageList.Add(packageById.Value);
                            }
                        }
                        else
                        {
                            result.Add(packageDependency);
                        }
                    }
                }
            }

            result.Reverse();
            return result;
        }

        internal static PackageRecord? GetPackageById(string id)
        {
            foreach (var packageGroupRecord in _recordArray.Groups)
            {
                foreach (var packageRecord in packageGroupRecord.Records)
                {
                    if (packageRecord.PackageId == id)
                        return packageRecord;
                }
            }

            return null;
        }
        
        internal static PackageGroupRecord[] GetPackageGroupRecords() => _recordArray.Groups;
    }
}