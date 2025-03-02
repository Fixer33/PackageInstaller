using UnityEngine.UIElements;

namespace PackageInstaller.Editor
{
    public class PackageGroupElement : VisualElement
    {
        private readonly PackageGroupRecord _record;

        internal PackageGroupElement(PackageGroupRecord packageGroupRecord, PackageElement[] elements)
        {
            _record = packageGroupRecord;

            this.Add(new Label()
            {
                name = "package-group__name-text",
                text = packageGroupRecord.Name
            });

            ScrollView packageContainer = new()
            {
                name = "package-group__container"
            };
            this.Add(packageContainer);
            for (var i = 0; i < elements.Length; i++)
            {
                packageContainer.contentContainer.Add(elements[i]);
            }
        }
    }
}