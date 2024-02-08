using System.IO;
using UnityEditor;
using UnityEngine;
using Utils;
using Utils.Attributes;

namespace SimpleDataEditor.Editor.Settings
{
    public class SimpleDataEditorSettings : ScriptableObjectSingleton<SimpleDataEditorSettings>
    {
        [SerializeField, PathSelector(isDirectory: true)]
        private string _generationFolder = "Assets/Scripts/Editor/Generated/SimpleDataEditors";
        public string GenerationFolder => _generationFolder;

        public static SimpleDataEditorSettings GetOrCreate()
        {
            if (Instance == null)
            {
                // create directory
                var dir = "Assets/Editor/SimpleDataEditor";
                Directory.CreateDirectory(dir);
                AssetDatabase.Refresh();

                // create asset
                var settings = CreateInstance<SimpleDataEditorSettings>();
                var path = Path.Combine(dir, "SimpleDataEditorSettings.asset");
                AssetDatabase.CreateAsset(settings, path);
                AssetDatabase.SaveAssets();
            }

            return Instance;
        }
    }
}