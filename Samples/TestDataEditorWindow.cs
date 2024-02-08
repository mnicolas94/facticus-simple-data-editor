    using SimpleDataEditor.Editor;
    using UnityEditor;
    using UnityEngine;

    namespace Samples
    {
        public class TestDataEditorWindow : DataTypeEditorWindow<TestData>
        {
            [MenuItem("Data/Test editor")]
            private static void ShowWindow()
            {
                var window = GetWindow<TestDataEditorWindow>();
                window.titleContent = new GUIContent("Test Data");
                window.Show();
            }
        }
    }