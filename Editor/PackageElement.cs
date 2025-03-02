using System;
using UnityEngine.UIElements;

namespace PackageInstaller.Editor
{
    /// <summary>
    /// Represents package element in the list
    /// </summary>
    internal class PackageElement : VisualElement
    {
        private const string IS_DEPENDENCY_CLASS_NAME = "dependencyPackage";
        private const string TO_BE_INSTALLED_CLASS_NAME = "toBeInstalled";
        
        public PackageRecord Record => _record;

        private readonly PackageRecord _record;
        private Action<PackageRecord, bool> _selectedStateChange;

        internal PackageElement(PackageRecord record, bool isInstalled, Action<PackageRecord, bool> selectedStateChange)
        {
            _record = record;
            _selectedStateChange = selectedStateChange;

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
                Toggle selectionToggle = new Toggle()
                {
                    name = "package-element__selection-toggle",
                };
                this.Add(selectionToggle);
                selectionToggle.RegisterValueChangedCallback(OnSelectionValueChanged);
            }
        }

        private void OnSelectionValueChanged(ChangeEvent<bool> evt)
        {
            _selectedStateChange?.Invoke(_record, evt.newValue);
        }

        public void HighlightAsDependency(bool isHighlighted)
        {
            if (isHighlighted)
            {
                this.AddToClassList(IS_DEPENDENCY_CLASS_NAME);
            }
            else
            {
                this.RemoveFromClassList(IS_DEPENDENCY_CLASS_NAME);
            }
        }
        
        public void HighlightAsToBeInstalled(bool isHighlighted)
        {
            if (isHighlighted)
            {
                this.AddToClassList(TO_BE_INSTALLED_CLASS_NAME);
            }
            else
            {
                this.RemoveFromClassList(TO_BE_INSTALLED_CLASS_NAME);
            }
        }
    }
}