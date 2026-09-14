using UnityEditor;
using UnityEngine;

namespace Editor {
    public class ConfigsToolbar : EditorWindow {
        [MenuItem("Tarotro/Configs/Show Toolbar", priority = 40)]
        private static void ShowWindow() {
            var window = GetWindow<ConfigsToolbar>("Configs");
            window.minSize = new Vector2(200, 30);
            window.maxSize = new Vector2(400, 30);
        }

        [InitializeOnLoadMethod]
        private static void AutoOpen() {
            EditorApplication.delayCall += () => {
                if (HasOpenInstances<ConfigsToolbar>()) return;
                ShowWindow();
            };
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();

            if (GUILayout.Button("Pull Configs", EditorStyles.toolbarButton))
                GoogleSheetsHelper.PullAll();

            if (GUILayout.Button("Open Sheet", EditorStyles.toolbarButton))
                GoogleSheetsHelper.OpenSpreadsheet();

            EditorGUILayout.EndHorizontal();
        }
    }
}
