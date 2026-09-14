using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Tarotro.Editor {
    [InitializeOnLoad]
    public static class PlayFromBootScene {
        private const string MenuPath = "Tarotro/Play From Boot Scene";
        private const string BootScenePath = "Assets/Scenes/boot.unity";
        private const string EditorPrefKey = "Tarotro_PlayFromBootScene";

        static PlayFromBootScene() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        [MenuItem(MenuPath, priority = 100)]
        private static void Toggle() {
            var enabled = !IsEnabled;
            EditorPrefs.SetBool(EditorPrefKey, enabled);
            Debug.Log($"Play From Boot Scene: {(enabled ? "ON" : "OFF")}");
        }

        [MenuItem(MenuPath, true)]
        private static bool ToggleValidate() {
            Menu.SetChecked(MenuPath, IsEnabled);
            return true;
        }

        private static bool IsEnabled => EditorPrefs.GetBool(EditorPrefKey, true);

        private static void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (!IsEnabled)
                return;

            if (state == PlayModeStateChange.ExitingEditMode) {
                var currentScene = EditorSceneManager.GetActiveScene().path;
                if (currentScene == BootScenePath)
                    return;

                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                    EditorSceneManager.playModeStartScene =
                        AssetDatabase.LoadAssetAtPath<SceneAsset>(BootScenePath);
                } else {
                    EditorApplication.isPlaying = false;
                }
            }

            if (state == PlayModeStateChange.EnteredEditMode) {
                EditorSceneManager.playModeStartScene = null;
            }
        }
    }
}
