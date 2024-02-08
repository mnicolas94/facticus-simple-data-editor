using UnityEditor;
using UnityEditor.UIElements;

namespace SimpleDataEditor.Editor.Settings
{
    public class SimpleDataEditorSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider GetSettingsProvider()
        {
            var settings = SimpleDataEditorSettings.GetOrCreate();
            SerializedObject so = new SerializedObject(settings);
            var keywords = SettingsProvider.GetSearchKeywordsFromSerializedObject(so);
        
            var provider = new SettingsProvider("Project/Facticus/Simple Data Editor", SettingsScope.Project)
            {
                activateHandler = (searchContext, root) =>
                {
                    var inspector = new InspectorElement(settings);
                    root.Add(inspector);
                },
                keywords = keywords
            };
        
            return provider;
        }
    }
}