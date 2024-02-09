using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleDataEditor.Editor.Settings;
using SimpleDataEditor.Editor.VisualElements;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace SimpleDataEditor.Editor
{
    public abstract class DataTypeEditorWindow<T> : EditorWindow where T : ScriptableObject
    {
        [SerializeField] private Object _selectedObject;
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        
        private InspectorElement _inspectorElement;
        private ScrollView _scrollView;
        private ListView _listView;
        private List<T> _data;
        private List<T> _filteredData;
        private ToolbarSearchField _searchField;
        private Button _reloadButton;
        private Button _addButton;
        private Button _removeButton;

        // protected abstract DataTypeEditorWindowSettings GetSettings();
        
        public void CreateGUI()
        {
            var root = rootVisualElement;
            
            // Instantiate UXML
            _visualTreeAsset ??= Resources.Load<VisualTreeAsset>("DataTypeEditorWindow");
            VisualElement visualTree = _visualTreeAsset.Instantiate();
            root.Add(visualTree);
            
            // setup PaneSplitView
            var splitView = new TwoPaneSplitView(0, 250, TwoPaneSplitViewOrientation.Horizontal);
            root.Add(splitView);
            var leftPanel = root.Q("LeftPanel");
            var rightPanel = root.Q("RightPanel");
            splitView.Add(leftPanel);
            splitView.Add(rightPanel);

            // setup data list
            _listView = root.Q<ListView>("DataList");
            CreateDataList();
            
            // search field
            _searchField = root.Q<ToolbarSearchField>("DataSearchField");
            _searchField.style.width = new StyleLength(StyleKeyword.Auto);
            _searchField.RegisterValueChangedCallback(OnSearchFieldChanged);
            
            // reload data button
            _reloadButton = root.Q<Button>("ReloadDataButton");
            _reloadButton.clicked += OnReloadDataButtonClicked;
            
            // add data button
            _addButton = root.Q<Button>("AddButton");
            _addButton.clicked += OnAddButtonClicked;
            
            // removee data button
            _removeButton = root.Q<Button>("RemoveButton");
            _removeButton.clicked += OnRemoveButtonClicked;
            
            // get reference to scroll view
            _scrollView = root.Q<ScrollView>("InspectorContainer");
            
            // display serialized value if any
            _listView.selectedIndex = _filteredData.IndexOf(_selectedObject as T);
            SelectObject(_selectedObject);
        }

        protected virtual List<T> LoadData()
        {
            var dataGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            var data = dataGuids
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .ToList();
            return data;
        }

        private void LoadDataAndPopulateListView()
        {
            _data = LoadData();
            _filteredData = new List<T>(_data);
            if (!Equals(_listView.itemsSource, _filteredData))
            {
                _listView.itemsSource = _filteredData;
            }
            else
            {
                _listView.RefreshItems();
            }
        }

        private void CreateDataList()
        {
            _listView.makeItem = CreateDataElement;
            _listView.bindItem = BindData;
            _listView.selectedIndicesChanged += OnDataListSelectedIndicesChanged;
            LoadDataAndPopulateListView();
        }

        private void BindData(VisualElement element, int i)
        {
            BindDataToView(element, i);
            ContextualMenuManipulator contextManipulator = new ContextualMenuManipulator(SetupContextMenu);
            contextManipulator.target = element;
        }
        
        private void SetupContextMenu(ContextualMenuPopulateEvent ctx)
        {
            var target = ctx.target as VisualElement;
            ctx.menu.AppendAction(
                "Ping",
                action => EditorGUIUtility.PingObject(target.userData as Object));
        }

        protected virtual void BindDataToView(VisualElement element, int i)
        {
            if (element is DataListElement dataElement)
            {
                dataElement.Data = _filteredData[i];
            }
        }

        protected virtual VisualElement CreateDataElement()
        {
            var field = new DataListElement();
            return field;
        }
        
        private void OnSearchFieldChanged(ChangeEvent<string> evt)
        {
            var value = evt.newValue;
            var lowerCaseValue = value.ToLower();
            var filteredData = _data.Where(d => d.name.ToLower().Contains(lowerCaseValue));
            _filteredData.Clear();
            _filteredData.AddRange(filteredData);
            _listView.RefreshItems();
        }
        
        private void OnReloadDataButtonClicked()
        {
            _reloadButton.SetEnabled(false);
            _searchField.SetValueWithoutNotify("");
            LoadDataAndPopulateListView();
            _reloadButton.SetEnabled(true);
        }
        
        private void OnAddButtonClicked()
        {
            // todo add undo support
            var newData = CreateInstance<T>();
            var packageSettings = SimpleDataEditorSettings.GetOrCreate();
            var editorSettings = packageSettings.GetOrCreateSettingsForEditorOfType(typeof(T));
            var path = Path.Combine(editorSettings.AssetCreationFolderPath, "New Data.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            AssetDatabase.CreateAsset(newData, path);
            AssetDatabase.Refresh();
            OnReloadDataButtonClicked();  // todo dont remove filtering
            // todo start renaming after creation
        }

        private void OnRemoveButtonClicked()
        {
            // todo add undo support
            var data = _filteredData[_listView.selectedIndex];
            var path = AssetDatabase.GetAssetPath(data);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.Refresh();
            OnReloadDataButtonClicked();  // todo dont remove filtering
        }

        private void OnDataListSelectedIndicesChanged(IEnumerable<int> indices)
        {
            var index = indices.FirstOrDefault();
            if (index >= 0)
            {
                SelectObject(_filteredData[index]);
            }
            else
            {
                SelectObject(null);
            }
        }

        private void SelectObject(Object obj)
        {
            _selectedObject = obj;
            
            var container = _scrollView.contentContainer;
            if (container.Contains(_inspectorElement))
            {
                container.Remove(_inspectorElement);
                _inspectorElement = null;
            }
            
            if (_selectedObject != null)
            {
                _inspectorElement = new InspectorElement(_selectedObject);
                container.Add(_inspectorElement);
            }
        }
    }
}
