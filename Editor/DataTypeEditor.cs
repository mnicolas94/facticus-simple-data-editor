using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace SimpleDataEditor.Editor
{
    public abstract class DataTypeEditor<T> : EditorWindow where T : ScriptableObject
    {
        [SerializeField] private Object _selectedObject;
        
        private Label _selectLabel;
        private ObjectField _objectSelector;
        private InspectorElement _inspectorElement;
        private ScrollView _scrollView;

        public void CreateGUI()
        {
            rootVisualElement.style.paddingTop = 12;
            rootVisualElement.style.paddingBottom = 12;
            rootVisualElement.style.paddingLeft = 12;
            rootVisualElement.style.paddingRight = 12;
            
            _selectLabel = new Label("Select an object");
            _selectLabel.style.marginBottom = 12;
            
            _objectSelector = new ObjectField();
            _objectSelector.objectType = typeof(T);
            _objectSelector.RegisterValueChangedCallback(OnObjectChanged);
            _objectSelector.value = _selectedObject;

            _scrollView = new ScrollView(ScrollViewMode.Vertical);
            
            rootVisualElement.Add(_selectLabel);
            rootVisualElement.Add(_objectSelector);
            rootVisualElement.Add(_scrollView);

            // display serialized value if any
            SelectObject(_selectedObject);
        }

        private void OnObjectChanged(ChangeEvent<Object> evt)
        {
            SelectObject(evt.newValue);
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