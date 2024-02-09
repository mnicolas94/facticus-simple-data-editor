using UnityEditor;
using UnityEngine;
using Utils.Attributes;

namespace SimpleDataEditor.Editor
{
    public class DataTypeEditorWindowSettings : ScriptableObject
    {
        [SerializeField, PathSelector(isDirectory: true)] private DefaultAsset _assetCreationFolder;

        public DefaultAsset AssetCreationFolder
        {
            get => _assetCreationFolder;
            set => _assetCreationFolder = value;
        }
    }
}