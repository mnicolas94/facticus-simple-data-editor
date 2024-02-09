using System;
using System.IO;
using System.Linq;
using System.Reflection;
using SimpleCodeGenerator.Editor;
using SimpleDataEditor.Editor.Settings;
using Unity.Plastic.Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Utils.Attributes;
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

            var fields = _inputData.GetType().GetRuntimeFields();
            foreach (var fieldInfo in fields)
            {
                DrawInputField(fieldInfo.Name);
            }
            
            // setup generate button
            var generateButton = root.Q<Button>("GenerateButton");
            generateButton.clicked += GenerateEditorWindowCode;
        }

        private void GenerateEditorWindowCode()
        {
            // generate editor window code
            var type = _inputData.Type.Type;

            var generationFolder = SimpleDataEditorSettings.GetOrCreate().GenerationFolder;
            generationFolder = Path.GetFullPath(generationFolder);
            
            // ensure generation folder is created
            if (!Directory.Exists(generationFolder))
            {
                Directory.CreateDirectory(generationFolder);
            }
            
            // generate assembly definition and add references
            GenerateAssembly(generationFolder, type);
            
            // generate editor code
            GenerateCodeEditor(generationFolder, type);
            
            // generate editor settings
            GenerateEditorSettings(generationFolder, type);
        }

        private void GenerateCodeEditor(string generationFolder, Type type)
        {
            var templateAsset = Resources.Load<TextAsset>("TemplateDataEditorWindow");
            var templatePath = AssetDatabase.GetAssetPath(templateAsset);
            var template = Template.ParseFromFile(templatePath);
            var scriptPath = GetEditorScriptPath(generationFolder, type);
            CodeGenerator.GenerateFromTemplate(template, scriptPath, _inputData);
        }

        private static void GenerateAssembly(string generationFolder, Type type)
        {
            var assemblyPath = GetAssemblyPath(generationFolder);
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

        private void GenerateEditorSettings(string generationFolder, Type type)
        {
            var settings = CreateInstance<DataTypeEditorWindowSettings>();
            settings.AssetCreationFolder = _inputData.AssetCreationFolder;
            var settingsPath = GetEditorSettingsPath(generationFolder, type);
            AssetDatabase.CreateAsset(settings, settingsPath);
            AssetDatabase.SaveAssetIfDirty(settings);
            
            var packageSettings = SimpleDataEditorSettings.GetOrCreate();
            packageSettings.RegisterSettingsForEditorOfType(type, settings);
        }

        private static string NormalizePathForCodeGenerator(string generationFolder)
        {
            // remove dataPath in order to work with CodeGenerator, as it adds dataPath to the start of output path
            var dataPath = Path.GetFullPath(Application.dataPath);
            if (generationFolder.StartsWith(dataPath))
            {
                generationFolder = Path.GetRelativePath(dataPath, generationFolder);
            }

            return generationFolder;
        }

        public static string GetAssemblyPath(string generationFolder)
        {
            return Path.Combine(generationFolder, "com.facticus.simple-data-editor.generated.asmdef");
        }

        public static string GetEditorScriptPath(string generationFolder, Type type)
        {
            generationFolder = NormalizePathForCodeGenerator(generationFolder);
            return Path.Combine(generationFolder, $"{type.FullName}EditorWindow.generated.cs");
        }
        
        public static string GetEditorSettingsPath(string generationFolder, Type type)
        {
            var editorSettingsPath = Path.Combine(generationFolder, $"{type.FullName}EditorWindow.settings.asset");
            // make relative for asset creation
            editorSettingsPath = Path.GetRelativePath("./", editorSettingsPath);
            return editorSettingsPath;
        }
    }
    
    [Serializable]
    public class InputData
    {
        [SerializeField] public TypeReference<Object> Type;
        public string TypeNamespace => Type.Type.Namespace;
        public string TypeName => Type.Type.Name;
        [SerializeField] public string MenuItemPath;
        [SerializeField] public string WindowTitle;
        [SerializeField, PathSelector(isDirectory: true)] public DefaultAsset AssetCreationFolder;
    }
}
