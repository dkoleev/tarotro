using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Tarotro.Editor {
    public static class DevCharacterSceneSetup {
        private const string ScenePath = "Assets/Scenes/dev/test_characters.unity";

        // [MenuItem("Tarotro/Create Dev Character Scene", priority = 200)]
        // private static void CreateScene() {
        //     if (System.IO.File.Exists(ScenePath)) {
        //         if (!EditorUtility.DisplayDialog(
        //                 "Scene Exists",
        //                 $"{ScenePath} already exists. Overwrite it?",
        //                 "Overwrite", "Cancel"))
        //             return;
        //     }
        //
        //     var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        //
        //     var camGo = new GameObject("Main Camera") {
        //         tag = "MainCamera"
        //     };
        //     var cam = camGo.AddComponent<Camera>();
        //     cam.orthographic = true;
        //     cam.orthographicSize = 1.8f;
        //     cam.backgroundColor = new Color(0.12f, 0.12f, 0.15f, 1f);
        //     cam.clearFlags = CameraClearFlags.SolidColor;
        //     cam.nearClipPlane = 0.3f;
        //     cam.farClipPlane = 1000f;
        //     camGo.transform.position = new Vector3(0, 0, -10);
        //
        //     camGo.AddComponent<AudioListener>();
        //
        //     var urpCamData = camGo.AddComponent<UniversalAdditionalCameraData>();
        //     urpCamData.renderShadows = false;
        //
        //     var pixelPerfect = camGo.AddComponent<PixelPerfectCamera>();
        //     pixelPerfect.assetsPPU = 100;
        //     pixelPerfect.refResolutionX = 420;
        //     pixelPerfect.refResolutionY = 270;
        //
        //     var testerGo = new GameObject("DevCharacterTester");
        //     testerGo.AddComponent<Game.Dev.DevCharacterTester>();
        //
        //     EditorSceneManager.SaveScene(scene, ScenePath);
        //     Debug.Log($"Dev character scene created at {ScenePath}");
        // }

        [MenuItem("Tarotro/Open Dev Character Scene", priority = 300)]
        private static void OpenDevCharacterScene() {
            OpenScene("Assets/Scenes/dev/test_characters.unity");
        }
        
        [MenuItem("Tarotro/Open Boot Scene", priority = 301)]
        private static void OpenBootScene() {
            OpenScene("Assets/Scenes/boot.unity");
        }
        
        [MenuItem("Tarotro/Open level_0 Scene", priority = 302)]
        private static void OpenLevel0Scene() {
            OpenScene("Assets/Scenes/level_0.unity");
        }

        private static void OpenScene(string path) {
            if (!System.IO.File.Exists(path)) {
                EditorUtility.DisplayDialog("Not Found", $"Scene not found at {path}.\nUse 'Tarotro > Create Dev Character Scene' first.", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene(path);
            }
        }
    }
}
