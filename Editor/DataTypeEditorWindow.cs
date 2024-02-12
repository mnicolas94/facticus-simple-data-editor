using System.Collections.Generic;
using SimpleDataEditor.Editor.Utility;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleDataEditor.Editor
{
    public abstract partial class DataTypeEditorWindow<T> : EditorWindow where T : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            
            // Instantiate UXML
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("DataTypeEditorWindow");
            VisualElement visualTree = _visualTreeAsset.Instantiate();
            visualTree.style.flexGrow = 1;
            root.Add(visualTree);
            
            var contextsToggle = root.Q<ToolbarToggle>("DataToggle");
            var settingsToggle = root.Q<ToolbarToggle>("SettingsToggle");
            var dataContainer = root.Q("DataContainer");
            var settingsContainer = root.Q("SettingsContainer");
            TabBarUtility.SetupTabBar(new List<(Toggle, VisualElement)>
            {
                (contextsToggle, dataContainer),
                (settingsToggle, settingsContainer)
            });
            
            SetupDataContainer(dataContainer);
            SetupSettingsContainer(settingsContainer);
        }
    }
}
