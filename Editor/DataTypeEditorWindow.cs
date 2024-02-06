using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SimpleDataEditor.Editor
{
    public abstract class DataTypeEditorWindow<T> : EditorWindow where T : ScriptableObject
    {
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        [SerializeField] private Object _selectedObject;
        
        private InspectorElement _inspectorElement;
        private ScrollView _scrollView;
        private ListView _listView;
        private List<T> _data;

        protected virtual List<T> LoadData()
        {
            var dataGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var data = dataGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();
            return data;
        }
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            
            // Instantiate UXML
            _visualTreeAsset = Resources.Load<VisualTreeAsset>("DataTypeEditorWindow");
            VisualElement visualTree = _visualTreeAsset.Instantiate();
            root.Add(visualTree);
            
            _listView = root.Q<ListView>("DataList");
            CreateDataList();
            _scrollView = root.Q<ScrollView>("InspectorContainer");
            
            // setup PaneSplitView
            var splitView = new TwoPaneSplitView(
                0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);
            splitView.Add(_listView);
            splitView.Add(_scrollView);
            
            // display serialized value if any
            SelectObject(_selectedObject);
        }

        private void CreateDataList()
        {
            _data = LoadData();
            _listView.makeItem = CreateDataElement;
            _listView.bindItem = BindDataToView;
            _listView.selectedIndicesChanged += OnDataListSelectedIndicesChanged;
            _listView.itemsSource = _data;
        }

        protected virtual void BindDataToView(VisualElement element, int i)
        {
            var objectField = element as ObjectField;
            objectField.objectType = typeof(T);
            objectField.value = _data[i];
        }

        protected virtual VisualElement CreateDataElement()
        {
            var field = new ObjectField();
            field.SetEnabled(false);
            return field;
        }

        private void OnDataListSelectedIndicesChanged(IEnumerable<int> indices)
        {
            var index = indices.FirstOrDefault();
            if (index >= 0)
            {
                SelectObject(_data[index]);
            }
            else
            {
                SelectObject(null);
            }
        }

        private void SelectObject(Object obj)
        {
            var container = _scrollView.contentContainer;
            if (container.Contains(_inspectorElement))
            {
                container.Remove(_inspectorElement);
                _inspectorElement = null;
            }

            _selectedObject = obj;
            if (_selectedObject != null)
            {
                _inspectorElement = new InspectorElement(_selectedObject);
                container.Add(_inspectorElement);
            }
        }
    }
}
