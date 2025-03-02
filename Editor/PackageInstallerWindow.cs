using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace PackageInstaller.Editor
{
    internal class PackageInstallerWindow : EditorWindow
    {
        private const float LOADING_INDICATOR_ROTATION_STEP = 30;
        
        [MenuItem("Tools/Fixer33/Package Installer", priority = 0)]
        private static void ShowWindow()
        {
            var window = GetWindow<PackageInstallerWindow>();
            window.titleContent = new GUIContent("Package Installer");
            window.Show();
        }

        private readonly List<PackageRecord> _packagesToInstall = new();
        private readonly List<PackageElement> _packageElements = new();
        private ScrollView _scrollView;
        private VisualElement _loadingIndicator;
        private VisualElement _loadingIndicatorPivot;
        private Button _installBtn;

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

            rootVisualElement.Add(_installBtn = new Button()
            {
                name = "install-btn",
                text = "Install"
            });
            _installBtn.clicked += OnInstallPress;
            _installBtn.SetEnabled(false);
            
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

        private void RefreshList()
        {
            CancellationTokenSource cancellationToken = AnimateLoading();

            if (PackageInstaller.RefreshInstalledPackages(installedPackages =>
                {
                    cancellationToken.Cancel();
                    rootVisualElement.Remove(_loadingIndicatorPivot);
                    List<PackageElement> elements = new();
                    foreach (var packageGroupRecord in PackageInstaller.GetPackageGroupRecords())
                    {
                        foreach (var packageRecord in packageGroupRecord.Records)
                        {
                            var packageElement = new PackageElement(packageRecord,
                                installedPackages.Any(i => i.Contains(packageRecord.PackageId)), PackageSelectionChanged);
                            elements.Add(packageElement);
                            _packageElements.Add(packageElement);
                        }
                        _scrollView.contentContainer.Add(new PackageGroupElement(packageGroupRecord, elements.ToArray()));
                        elements.Clear();
                    }
                    
                }) == false)
                cancellationToken.Cancel();
        }

        private void PackageSelectionChanged(PackageRecord packageRecord, bool isSelected)
        {
            if (isSelected)
            {
                if (_packagesToInstall.Contains(packageRecord) == false)
                    _packagesToInstall.Add(packageRecord);
            }
            else
            {
                if (_packagesToInstall.Contains(packageRecord))
                    _packagesToInstall.Remove(packageRecord);
            }

            List<string> dependencies = PackageInstaller.GetPackagesWithDependencies(_packagesToInstall.ToArray());
            
            foreach (var packageElement in _packageElements)
            {
                packageElement.HighlightAsDependency(dependencies.Contains(packageElement.Record.PackageId));
                packageElement.HighlightAsToBeInstalled(_packagesToInstall.Contains(packageElement.Record));
            }
            
            _installBtn.SetEnabled(_packagesToInstall.Count > 0);
        }

        private void OnInstallPress()
        {
            var cancellationToken = AnimateLoading();
            PackageInstaller.InstallPackages(_packagesToInstall.ToArray(), () =>
            {
                _packagesToInstall.Clear();
                cancellationToken.Cancel();
                RefreshList();
            });
        }
    }
}