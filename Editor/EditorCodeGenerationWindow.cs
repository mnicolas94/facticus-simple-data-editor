using System;
using SimpleCodeGenerator.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Serializables;
using Object = UnityEngine.Object;

namespace SimpleDataEditor.Editor
{
    public class EditorCodeGenerationWindow : EditorWindow
    {
        [SerializeField] private VisualTreeAsset _visualTreeAsset;
        [SerializeField] private InputData _inputData;

        [MenuItem("Tools/Facticus.Simple Data Editor/Generate Editor Window Code")]
        public static void ShowExample()
        {
            EditorCodeGenerationWindow wnd = GetWindow<EditorCodeGenerationWindow>();
            wnd.titleContent = new GUIContent("EditorCodeGenerationWindow");
        }

        public void CreateGUI()
        {
            var root = rootVisualElement;

            // Instantiate UXML
            _visualTreeAsset = Resources.Load<VisualTreeAsset>("EditorCodeGenerationWindow");
            VisualElement visualTree = _visualTreeAsset.Instantiate();
            root.Add(visualTree);

            // setup input
            var inputContainer = root.Q("InputContainer");
            var serializedObject = new SerializedObject(this);

            void DrawInputField(string path)
            {
                var field = new PropertyField();
                field.bindingPath = $"{nameof(_inputData)}.{path}";
                field.Bind(serializedObject);
                inputContainer.Add(field);
            }
            
            DrawInputField(nameof(_inputData.Type));
            DrawInputField(nameof(_inputData.MenuItemPath));
            DrawInputField(nameof(_inputData.WindowTitle));
            
            // setup generate button
            var generateButton = root.Q<Button>("GenerateButton");
            generateButton.clicked += GenerateEditorWindowCode;
        }
        
        public void GenerateEditorWindowCode()
        {
            var data = new
            {
                TypeNamespace = _inputData.Type.Type.Namespace,
                TypeName = _inputData.Type.Type.Name,
                MenuItemPath = _inputData.MenuItemPath,
                WindowTitle = _inputData.WindowTitle,
            };
            
            var templateAsset = Resources.Load<TextAsset>("DataEditorWindowTemplate");
            var templatePath = AssetDatabase.GetAssetPath(templateAsset);
            var scriptPath = "Scripts/Generated/MyTemplate.generated.cs";  // TODO get from settings
            CodeGenerator.GenerateFromTemplate(templatePath, scriptPath, data);
        }
    }
    
    [Serializable]
    public class InputData
    {
        [SerializeField] public TypeReference<Object> Type;
        [SerializeField] public string MenuItemPath;
        [SerializeField] public string WindowTitle;
    }
}
