using System;
using System.Linq;
using Tarotro.Editor.Validation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build.Profile;
using UnityEngine;

namespace Tarotro.Editor.Build {
    public static class GameBuilder {
        private const string BuildConfigsPath = "Assets/Settings/BuildConfigs/BuildConfigList.asset";

        [MenuItem("Tarotro/Build/Build Default", priority = 0)]
        public static void BuildDefault() {
            var list = LoadConfigsList();
            if (list == null) return;

            if (list.defaultConfig == null) {
                Debug.LogError("[Build] No default config set. Assign one in the BuildConfigsList Inspector.");
                return;
            }

            Build(list.defaultConfig);
        }

        [MenuItem("Tarotro/Build/Build Development", priority = 1)]
        public static void BuildDevelopment() {
            var list = LoadConfigsList();
            if (list == null) return;

            if (list.developmentConfig == null) {
                Debug.LogError("[Build] No development config set. Assign one in the BuildConfigsList Inspector.");
                return;
            }

            Build(list.developmentConfig);
        }

        [MenuItem("Tarotro/Build/Build Selected Config", priority = 2)]
        public static void BuildSelected() {
            var config = Selection.activeObject as BuildConfig;
            if (config == null) {
                Debug.LogError("[Build] Select a BuildConfig asset in the Project window first.");
                return;
            }

            Build(config);
        }

        [MenuItem("Tarotro/Build/Build Selected Config", true)]
        private static bool BuildSelectedValidate() {
            return Selection.activeObject is BuildConfig;
        }

        [MenuItem("Tarotro/Build/Build All Configs", priority = 3)]
        public static void BuildAll() {
            var list = LoadConfigsList();
            if (list == null) return;

            var succeeded = 0;
            var failed = 0;

            foreach (var config in list.configs) {
                if (config == null) continue;

                Debug.Log($"[Build] === Building profile: {config.name} ===");
                if (Build(config))
                    succeeded++;
                else
                    failed++;
            }

            Debug.Log($"[Build] Finished. Succeeded: {succeeded}, Failed: {failed}");
        }

        [MenuItem("Tarotro/Build/Open Build Configs", priority = 20)]
        public static void OpenBuildConfigs() {
            var list = LoadConfigsList();
            if (list != null)
                Selection.activeObject = list;
        }

        public static bool Build(BuildConfig config) {
            var variant = config.isDevelopment ? "Development" : "Release";
            Debug.Log($"[Build] Starting '{config.name}' ({variant})...");

            if (config.buildProfile == null) {
                Debug.LogError($"[Build] No Build Profile assigned in '{config.name}'. Assign one in the Inspector.");
                return false;
            }

            if (config.validateConfigs && !RunConfigValidation())
                return false;

            if (config.buildAddressables && !RunAddressablesBuild())
                return false;

            return RunPlayerBuild(config);
        }

        private static BuildConfigsList LoadConfigsList() {
            var list = AssetDatabase.LoadAssetAtPath<BuildConfigsList>(BuildConfigsPath);
            if (list != null) return list;

            Debug.LogError(
                $"[Build] BuildConfigsList not found at '{BuildConfigsPath}'. " +
                "Create one via Assets > Create > Tarotro > Build Configs List.");
            return null;
        }

        private static bool RunConfigValidation() {
            Debug.Log("[Build] Running config validation...");
            var result = ConfigValidationRunner.RunAll();

            if (result.IsValid) {
                Debug.Log("[Build] Config validation passed.");
                return true;
            }

            foreach (var error in result.Errors)
                Debug.LogError($"[Build] Config: {error}");

            foreach (var warning in result.Warnings)
                Debug.LogWarning($"[Build] Config: {warning}");

            Debug.LogError($"[Build] Config validation failed with {result.Errors.Count} error(s). Aborting build.");
            return false;
        }

        private static bool RunAddressablesBuild() {
            Debug.Log("[Build] Building Addressables...");

            try {
                AddressableAssetSettings.CleanPlayerContent(
                    AddressableAssetSettingsDefaultObject.Settings.ActivePlayerDataBuilder);

                AddressableAssetSettings.BuildPlayerContent(out var buildResult);

                if (!string.IsNullOrEmpty(buildResult.Error)) {
                    Debug.LogError($"[Build] Addressables build failed: {buildResult.Error}");
                    return false;
                }

                Debug.Log($"[Build] Addressables built successfully. Duration: {buildResult.Duration:F1}s");
                return true;
            } catch (Exception e) {
                Debug.LogError($"[Build] Addressables build exception: {e.Message}");
                return false;
            }
        }

        private static bool RunPlayerBuild(BuildConfig config) {
            Debug.Log($"[Build] Building player to '{config.OutputPath}'...");

            var options = config.isDevelopment ? BuildOptions.Development : BuildOptions.None;

            var savedDefines = ApplyExtraDefines(config);

            try {
                var buildOptions = new BuildPlayerWithProfileOptions {
                    buildProfile = config.buildProfile,
                    locationPathName = config.OutputPath,
                    options = options
                };

                var report = BuildPipeline.BuildPlayer(buildOptions);

                if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded) {
                    var size = report.summary.totalSize / (1024f * 1024f);
                    Debug.Log($"[Build] Build succeeded! Size: {size:F1} MB, Time: {report.summary.totalTime}");
                    return true;
                }

                Debug.LogError($"[Build] Build failed: {report.summary.result}");
                foreach (var step in report.steps) {
                    foreach (var msg in step.messages) {
                        if (msg.type == LogType.Error)
                            Debug.LogError($"[Build] {msg.content}");
                    }
                }

                return false;
            } finally {
                RestoreDefines(savedDefines);
            }
        }

        private static string ApplyExtraDefines(BuildConfig config) {
            if (config.extraScriptingDefines == null || config.extraScriptingDefines.Length == 0)
                return null;

            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            var current = PlayerSettings.GetScriptingDefineSymbols(namedTarget);
            var currentDefines = current.Split(';').ToList();

            foreach (var define in config.extraScriptingDefines) {
                var trimmed = define.Trim();
                if (!string.IsNullOrEmpty(trimmed) && !currentDefines.Contains(trimmed))
                    currentDefines.Add(trimmed);
            }

            PlayerSettings.SetScriptingDefineSymbols(namedTarget, string.Join(";", currentDefines));
            Debug.Log($"[Build] Applied extra defines: {string.Join(", ", config.extraScriptingDefines)}");
            return current;
        }

        private static void RestoreDefines(string savedDefines) {
            if (savedDefines == null) return;

            var namedTarget = NamedBuildTarget.FromBuildTargetGroup(
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget));
            PlayerSettings.SetScriptingDefineSymbols(namedTarget, savedDefines);
            Debug.Log("[Build] Restored original scripting defines.");
        }
    }
}
