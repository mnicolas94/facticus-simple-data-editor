using System;
using System.IO;
using System.Linq;
using SimpleCodeGenerator.Editor;
using Unity.Plastic.Newtonsoft.Json.Linq;
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

        private void GenerateEditorWindowCode()
        {
            // generate editor window code
            var type = _inputData.Type.Type;
            var data = new
            {
                TypeNamespace = type.Namespace,
                TypeName = type.Name,
                MenuItemPath = _inputData.MenuItemPath, 
                WindowTitle = _inputData.WindowTitle,
            };
            
            var generationFolder = Path.GetFullPath("Assets/Scripts/Generated");  // TODO get from settings
            // remove dataPath in order to work with CodeGenerator, as it adds dataPath to the start of output path
            var dataPath = Path.GetFullPath(Application.dataPath);
            if (generationFolder.StartsWith(dataPath))
            {
                generationFolder = Path.GetRelativePath(dataPath, generationFolder);
            }
            
            var templateAsset = Resources.Load<TextAsset>("TemplateDataEditorWindow");
            var templatePath = AssetDatabase.GetAssetPath(templateAsset);
            var scriptPath = Path.Combine(generationFolder, $"{type.FullName}EditorWindow.generated.cs");
            CodeGenerator.GenerateFromTemplate(templatePath, scriptPath, data);
            
            // generate assembly definition and add references
            var assemblyPath = Path.Combine(Application.dataPath, generationFolder, "com.facticus.simple-data-editor.generated.asmdef");
            var assemblyExists = File.Exists(assemblyPath);
            var assemblyContent = "";
            if (assemblyExists)
            {
                assemblyContent = File.ReadAllText(assemblyPath);
            }
            else
            {
                var assemblyTemplateAsset = Resources.Load<TextAsset>("TemplateAssembly");
                assemblyContent = assemblyTemplateAsset.text;
            }

            var json = JObject.Parse(assemblyContent);
            var reference = type.Assembly.GetName().Name;
            if (json["references"] is JArray references)
            {
                var cleanReferences = references.Select(r => r.ToString().Trim());
                if (!cleanReferences.Contains(reference))
                {
                    references.Add(reference);
                }
            }
            var newContent = json.ToString();
            File.WriteAllText(assemblyPath, newContent);
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
