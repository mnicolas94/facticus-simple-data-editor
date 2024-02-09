using System;
using Deploy.Editor.VisualElements;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SimpleDataEditor.Editor.VisualElements
{
    public class DataListElement : VisualElement
    {
        private static VisualTreeAsset _uxml;
        private static VisualTreeAsset Uxml => _uxml ??= Resources.Load<VisualTreeAsset>("DataListElement");
        
        private Object _data;
        private RenamableLabel _label;

        public DataListElement()
        {
            var visualElement = Uxml.Instantiate();
            Add(visualElement);

            _label = new RenamableLabel("");
            _label.OnRename += OnRename;
            
            var labelContainer = visualElement.Q("LabelContainer");
            labelContainer.Add(_label);
        }

        private void OnRename(string newName)
        {
            // TODO implement undo/redo support
            // var previousName = _data.name;
            // Undo.RegisterCompleteObjectUndo(_data, $"Rename data element:{previousName}->{newName}");
            var path = AssetDatabase.GetAssetPath(_data);
            AssetDatabase.RenameAsset(path, newName);
            EditorUtility.SetDirty(_data);
        }

        public Object Data
        {
            get => _data;
            set
            {
                _data = value;
                userData = value;
                _label.Text = _data.name;
            }
        }
    }
}