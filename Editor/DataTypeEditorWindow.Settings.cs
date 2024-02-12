using System.Collections.Generic;
using SimpleDataEditor.Editor.Settings;
using SimpleDataEditor.Editor.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleDataEditor.Editor
{
    public abstract partial class DataTypeEditorWindow<T> where T : ScriptableObject
    {
        private void SetupSettingsContainer(VisualElement root)
        {
            var settingsPackage = SimpleDataEditorSettings.GetOrCreate();
            var settings = settingsPackage.GetOrCreateSettingsForEditorOfType(typeof(T));
            var settingsInspector = new InspectorElement(settings);
            root.Add(settingsInspector);
        }
    }
}
