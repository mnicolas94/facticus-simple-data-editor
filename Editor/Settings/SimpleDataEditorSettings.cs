using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Utils.Editor;
using Utils.Attributes;
using Utils.Serializables;
using Object = UnityEngine.Object;

namespace SimpleDataEditor.Editor.Settings
{
    public class SimpleDataEditorSettings : ScriptableObjectSingleton<SimpleDataEditorSettings>
    {
        [SerializeField, PathSelector(isDirectory: true)]
        private string _generationFolder = "Assets/Scripts/Editor/Generated/SimpleDataEditors";
        public string GenerationFolder => _generationFolder;

        [SerializeField, HideInInspector]
        private List<TypeToSettingsTuple> _editorsSettings = new();

        public DataTypeEditorWindowSettings GetOrCreateSettingsForEditorOfType(Type type)
        {
            bool SearchPredicate(TypeToSettingsTuple tuple)
            {
                return tuple.Type.Type == type;
            }

            if (!_editorsSettings.Exists(SearchPredicate))
            {
                RegisterSettingsForEditorOfType(type, null);
            }

            var tuple = _editorsSettings.Find(SearchPredicate);
            var (serializableType, settings) = tuple;
            if (settings == null)
            {
                settings = CreateInstance<DataTypeEditorWindowSettings>();
                string path = EditorCodeGenerationWindow.GetEditorSettingsPath(_generationFolder, type);
                AssetDatabase.CreateAsset(settings, path);
                tuple.Settings = settings;
                EditorUtility.SetDirty(this);
            }

            return settings;
        }

        private void RegisterSettingsForEditorOfType(Type type, DataTypeEditorWindowSettings settings)
        {
            var tuple = new TypeToSettingsTuple(type, settings);
            _editorsSettings.Add(tuple);
            EditorUtility.SetDirty(this);
        }
        
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

    [Serializable]
    public class TypeToSettingsTuple
    {
        [SerializeField] public TypeReference<Object> Type;
        [SerializeField] public DataTypeEditorWindowSettings Settings;

        public TypeToSettingsTuple(Type type, DataTypeEditorWindowSettings settings)
        {
            var serializableType = new TypeReference<Object>
            {
                Type = type
            };
            Type = serializableType;
            Settings = settings;
        }
        
        public TypeToSettingsTuple(TypeReference<Object> type, DataTypeEditorWindowSettings settings)
        {
            Type = type;
            Settings = settings;
        }

        public void Deconstruct(out TypeReference<Object> type, out DataTypeEditorWindowSettings settings)
        {
            type = Type;
            settings = Settings;
        }
    }
}