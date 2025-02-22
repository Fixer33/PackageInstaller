using System;
using UnityEngine.UIElements;

namespace PackageInstaller.Editor
{
    internal class PackageElement : VisualElement
    {
        private PackageRecord _record;

        internal PackageElement(PackageRecord record, bool isInstalled, Action<PackageRecord> installClicked)
        {
            _record = record;

            this.Add(new Label()
            {
                name = "package-element__name-text",
                text = record.PackageName
            });

            if (isInstalled)
            {
                this.Add(new Label()
                {
                    name = "package-element__installed-text",
                    text = "Installed"
                });
            }
            else
            {
                Button btn = new Button()
                {
                    name = "package-element__install-btn",
                    text = "Install"
                };
                btn.clicked += () =>
                {
                    installClicked?.Invoke(record);
                };
                this.Add(btn);
            }
        }
    }
}