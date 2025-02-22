using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PackageInstaller.Editor
{
    public class PackageInstallerWindow : EditorWindow
    {
        private const float LOADING_INDICATOR_ROTATION_STEP = 30;
        
        [MenuItem("Tools/Fixer33/Package installer", priority = 0)]
        private static void ShowWindow()
        {
            var window = GetWindow<PackageInstallerWindow>();
            window.titleContent = new GUIContent("Package installer");
            window.Show();
        }

        private ScrollView _scrollView;
        private VisualElement _loadingIndicator;
        private VisualElement _loadingIndicatorPivot;

        private void CreateGUI()
        {
            string loadPath = "Packages/com.fixer33.package-installer/Editor/Styles/PackageInstallerWindow.uss";
#if PACKAGES_DEV
            loadPath = "Assets/" + loadPath;            
#endif
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(loadPath);
            if (style != null)
            {
                rootVisualElement.styleSheets.Add(style);
            }
            
            rootVisualElement.Add(new Label()
            {
                name = "header",
                text = "Package Installer"
            });
            
            rootVisualElement.Add(_scrollView = new ScrollView()
            {
                name = "container"
            });

            string loadingIndicator = "Packages/com.fixer33.package-installer/Editor/Textures/ic_loading.png";
#if PACKAGES_DEV
            loadingIndicator = "Assets/" + loadingIndicator;            
#endif

            _loadingIndicatorPivot = new VisualElement()
            {
                name = "loading-indicator-pivot"
            };
            
            _loadingIndicatorPivot.Add(_loadingIndicator = new VisualElement()
            {
                name = "loading-indicator",
                style = { backgroundImage = AssetDatabase.LoadAssetAtPath<Texture2D>(loadingIndicator)}
            });
            
            RefreshList();
        }

        private void RefreshList()
        {
            CancellationTokenSource cancellationToken = AnimateLoading();

            if (PackageInstaller.RefreshInstalledPackages(installedPackages =>
                {
                    cancellationToken.Cancel();
                    rootVisualElement.Remove(_loadingIndicatorPivot);
                    foreach (var packageRecord in PackageInstaller.GetPackageRecords())
                    {
                        _scrollView.contentContainer.Add(new PackageElement(packageRecord,
                            installedPackages.Any(i => i.Contains(packageRecord.PackageId)), InstallPackageClicked));
                    }
                }) == false)
                cancellationToken.Cancel();
        }

        private CancellationTokenSource AnimateLoading()
        {
            _scrollView.contentContainer.Clear();
            if (rootVisualElement.Children().Contains(_loadingIndicatorPivot))
                rootVisualElement.Remove(_loadingIndicatorPivot);
            rootVisualElement.Add(_loadingIndicatorPivot);
            CancellationTokenSource cancellationToken = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (this == true)
                {
                    _loadingIndicator.transform.rotation *= Quaternion.Euler(0, 0, LOADING_INDICATOR_ROTATION_STEP);
                    await Task.Delay(100, cancellationToken.Token);
                }
            }, cancellationToken.Token);

            return cancellationToken;
        }

        private void InstallPackageClicked(PackageRecord packageRecord)
        {
            var cancellationToken = AnimateLoading();
            PackageInstaller.InstallPackage(packageRecord, () =>
            {
                cancellationToken.Cancel();
                RefreshList();
            });
        }
    }
}