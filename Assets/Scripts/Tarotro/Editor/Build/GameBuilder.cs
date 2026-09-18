using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Tarotro.Editor.Validation;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.Build;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Tarotro.Editor.Build {
    public static class GameBuilder {
        private const string BuildConfigsPath = "Assets/Settings/BuildConfigs/BuildConfigList.asset";
        private static readonly List<string> LogBuffer = new();

        [MenuItem("Tarotro/Build/Windows Release", priority = 0)]
        public static void BuildWindowsRelease() {
            var list = LoadConfigsList();
            if (list == null) return;

            if (list.windowsReleaseConfig == null) {
                Debug.LogError("[Build] No default config set. Assign one in the BuildConfigsList Inspector.");
                return;
            }

            Build(list.windowsReleaseConfig);
        }

        [MenuItem("Tarotro/Build/Windows Development", priority = 1)]
        public static void BuildWindowsDevelopment() {
            var list = LoadConfigsList();
            if (list == null) return;

            if (list.windowsDevelopmentConfig == null) {
                Debug.LogError("[Build] No development config set. Assign one in the BuildConfigsList Inspector.");
                return;
            }

            Build(list.windowsDevelopmentConfig);
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

        [MenuItem("Tarotro/Build/Open Build Configs", priority = 20)]
        public static void OpenBuildConfigs() {
            var list = LoadConfigsList();
            if (list != null)
                Selection.activeObject = list;
        }

        public static void BuildCI() {
            var list = LoadConfigsList();
            if (list?.windowsReleaseConfig == null) {
                Debug.LogError("[Build] No release config available for CI build.");
                EditorApplication.Exit(1);
                return;
            }

            Build(list.windowsReleaseConfig, success => {
                if (!success)
                    EditorApplication.Exit(1);
            });
        }

        public static void Build(BuildConfig config, Action<bool> onComplete = null) {
            var variant = config.isDevelopment ? "Development" : "Release";
            Debug.Log($"[Build] Starting '{config.name}' ({variant})...");

            if (config.buildProfile == null) {
                Debug.LogError($"[Build] No Build Profile assigned in '{config.name}'. Assign one in the Inspector.");
                onComplete?.Invoke(false);
                return;
            }

            var stopwatch = Stopwatch.StartNew();

            if (config.saveBuildLog)
                StartLogCapture();

            void CompleteBuild(bool success) {
                stopwatch.Stop();
                var elapsed = stopwatch.Elapsed;
                Debug.Log($"[Build] Total pipeline time: {(int)elapsed.TotalMinutes}m {elapsed.Seconds}s");

                if (config.saveBuildLog) {
                    StopLogCapture();
                    SaveBuildLog(config);
                }

                if (success) {
                    if (config.openFolderAfterBuild && !Application.isBatchMode)
                        OpenBuildFolder(config);

                    if (config.uploadToSteam)
                        RunSteamUpload(config);
                }

                onComplete?.Invoke(success);
            }

            RunPipeline(config, CompleteBuild);
        }

        private static void RunPipeline(BuildConfig config, Action<bool> onComplete) {
            if (config.runTests) {
                EditModeTestRunner.RunAsync(testsPassed => {
                    if (!testsPassed) {
                        onComplete(false);
                        return;
                    }

                    onComplete(RunPipelinePostTests(config));
                });
                return;
            }

            onComplete(RunPipelinePostTests(config));
        }

        private static bool RunPipelinePostTests(BuildConfig config) {
            if (config.autoIncrementBuildNumber)
                IncrementBuildNumber();

            if (config.validateConfigs && !RunConfigValidation())
                return false;

            if (config.buildAddressables && !RunAddressablesBuild())
                return false;

            if (config.cleanBeforeBuild)
                CleanOutputFolder(config);

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

        private static void IncrementBuildNumber() {
            var version = PlayerSettings.bundleVersion;
            var parts = version.Split('.');

            if (parts.Length >= 2 && int.TryParse(parts[^1], out var buildNum)) {
                parts[^1] = (buildNum + 1).ToString();
                var newVersion = string.Join(".", parts);
                PlayerSettings.bundleVersion = newVersion;
                Debug.Log($"[Build] Version incremented: {version} -> {newVersion}");
            } else {
                Debug.LogWarning($"[Build] Could not parse version '{version}' for auto-increment. Skipping.");
            }
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

        private static void CleanOutputFolder(BuildConfig config) {
            var folder = config.OutputFolder;
            if (!Directory.Exists(folder)) return;

            Debug.Log($"[Build] Cleaning output folder: {folder}");
            Directory.Delete(folder, true);
            Directory.CreateDirectory(folder);
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

        private static void OpenBuildFolder(BuildConfig config) {
            Debug.Log($"[Build] Opening build folder: {config.OutputFolder}");
            EditorUtility.RevealInFinder(config.OutputPath);
        }

        private static void RunSteamUpload(BuildConfig config) {
            if (string.IsNullOrEmpty(config.steamCmdPath) || string.IsNullOrEmpty(config.steamVdfPath)) {
                Debug.LogWarning("[Build] Steam upload skipped: SteamCMD path or VDF path not configured.");
                return;
            }

            if (!File.Exists(config.steamCmdPath)) {
                Debug.LogError($"[Build] SteamCMD not found at '{config.steamCmdPath}'.");
                return;
            }

            if (!File.Exists(config.steamVdfPath)) {
                Debug.LogError($"[Build] Steam VDF file not found at '{config.steamVdfPath}'.");
                return;
            }

            Debug.Log("[Build] Starting Steam upload...");

            var vdfFullPath = Path.GetFullPath(config.steamVdfPath);
            var args = $"+login {config.steamUsername} +run_app_build \"{vdfFullPath}\" +quit";

            try {
                var process = new Process {
                    StartInfo = new ProcessStartInfo {
                        FileName = config.steamCmdPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.Start();
                var output = process.StandardOutput.ReadToEnd();
                var error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                if (process.ExitCode == 0) {
                    Debug.Log($"[Build] Steam upload completed.\n{output}");
                } else {
                    Debug.LogError($"[Build] Steam upload failed (exit code {process.ExitCode}).\n{output}\n{error}");
                }
            } catch (Exception e) {
                Debug.LogError($"[Build] Steam upload exception: {e.Message}");
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

        private static void StartLogCapture() {
            LogBuffer.Clear();
            Application.logMessageReceived += OnLogMessage;
        }

        private static void StopLogCapture() {
            Application.logMessageReceived -= OnLogMessage;
        }

        private static void OnLogMessage(string message, string stackTrace, LogType type) {
            var prefix = type switch {
                LogType.Error => "ERROR",
                LogType.Warning => "WARN",
                LogType.Exception => "EXCEPTION",
                _ => "INFO"
            };
            LogBuffer.Add($"[{DateTime.Now:HH:mm:ss}] [{prefix}] {message}");
            if (type == LogType.Exception && !string.IsNullOrEmpty(stackTrace))
                LogBuffer.Add(stackTrace);
        }

        private static void SaveBuildLog(BuildConfig config) {
            try {
                var folder = config.OutputFolder;
                Directory.CreateDirectory(folder);
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                var logPath = Path.Combine(folder, $"build_log_{timestamp}.txt");
                File.WriteAllLines(logPath, LogBuffer);
                Debug.Log($"[Build] Log saved to '{logPath}'");
            } catch (Exception e) {
                Debug.LogError($"[Build] Failed to save build log: {e.Message}");
            }
        }
    }
}
