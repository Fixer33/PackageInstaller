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

        private static AddRequest _addRequest;
        private static Action _addRequestCallback;

        internal static void InstallPackage(PackageRecord package, Action callback)
        {
            if (_addRequest is { IsCompleted: false })
                return;
            
            _addRequestCallback = callback;
            _addRequest = Client.Add(package.PackageUrl);
            EditorApplication.update -= InstallPackageProgress;
            EditorApplication.update += InstallPackageProgress;
        }

        private static void InstallPackageProgress()
        {
            if (_addRequest.IsCompleted == false)
                return;
            
            EditorApplication.update -= InstallPackageProgress;
            _addRequestCallback?.Invoke();
            _addRequestCallback = null;
            _addRequest = null;
        }

        #endregion

        internal static PackageRecord[] GetPackageRecords() => _recordArray.Records;
    }
}