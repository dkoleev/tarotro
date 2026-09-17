using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Tarotro.Editor {
    public static class DevCharacterSceneSetup {
        private const string ScenePath = "Assets/Scenes/dev_characters.unity";

        [MenuItem("Tarotro/Create Dev Character Scene", priority = 200)]
        private static void CreateScene() {
            if (System.IO.File.Exists(ScenePath)) {
                if (!EditorUtility.DisplayDialog(
                        "Scene Exists",
                        $"{ScenePath} already exists. Overwrite it?",
                        "Overwrite", "Cancel"))
                    return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var camGo = new GameObject("Main Camera") {
                tag = "MainCamera"
            };
            var cam = camGo.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = 1.8f;
            cam.backgroundColor = new Color(0.12f, 0.12f, 0.15f, 1f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.nearClipPlane = 0.3f;
            cam.farClipPlane = 1000f;
            camGo.transform.position = new Vector3(0, 0, -10);

            camGo.AddComponent<AudioListener>();

            var urpCamData = camGo.AddComponent<UniversalAdditionalCameraData>();
            urpCamData.renderShadows = false;

            var pixelPerfect = camGo.AddComponent<PixelPerfectCamera>();
            pixelPerfect.assetsPPU = 100;
            pixelPerfect.refResolutionX = 420;
            pixelPerfect.refResolutionY = 270;

            var testerGo = new GameObject("DevCharacterTester");
            testerGo.AddComponent<Game.Dev.DevCharacterTester>();

            EditorSceneManager.SaveScene(scene, ScenePath);
            Debug.Log($"Dev character scene created at {ScenePath}");
        }

        [MenuItem("Tarotro/Open Dev Character Scene", priority = 201)]
        private static void OpenScene() {
            if (!System.IO.File.Exists(ScenePath)) {
                EditorUtility.DisplayDialog("Not Found", $"Scene not found at {ScenePath}.\nUse 'Tarotro > Create Dev Character Scene' first.", "OK");
                return;
            }

            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) {
                EditorSceneManager.OpenScene(ScenePath);
            }
        }
    }
}
