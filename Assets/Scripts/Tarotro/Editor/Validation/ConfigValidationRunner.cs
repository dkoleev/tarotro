using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Tarotro.Editor.Validation {
    public static class ConfigValidationRunner {
        private static List<IConfigValidator> GetValidators() {
            return new List<IConfigValidator> {
                new CircleEnemyTypesValidator()
            };
        }

        [MenuItem("Tarotro/Validation/Validate All Configs", priority = 0)]
        public static void ValidateAll() {
            var result = RunAll();
            LogResult(result);

            if (result.IsValid) {
                EditorUtility.DisplayDialog("Config Validation", "All configs are valid.", "OK");
            } else {
                EditorUtility.DisplayDialog("Config Validation",
                    $"Validation failed with {result.Errors.Count} error(s). Check the Console for details.",
                    "OK");
            }
        }

        public static ConfigValidationResult RunAll() {
            var combined = new ConfigValidationResult();
            foreach (var validator in GetValidators()) {
                Debug.Log($"[ConfigValidation] Running: {validator.Name}");
                var result = validator.Validate();
                combined.Merge(result);
            }

            return combined;
        }

        private static void LogResult(ConfigValidationResult result) {
            foreach (var warning in result.Warnings) {
                Debug.LogWarning($"[ConfigValidation] {warning}");
            }

            foreach (var error in result.Errors) {
                Debug.LogError($"[ConfigValidation] {error}");
            }

            if (result.IsValid) {
                Debug.Log("[ConfigValidation] All configs are valid.");
            }
        }
    }

    public class ConfigValidationBuildCheck : IPreprocessBuildWithReport {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report) {
            var result = ConfigValidationRunner.RunAll();
            if (!result.IsValid) {
                var message = $"Config validation failed with {result.Errors.Count} error(s). Check the Console.";
                throw new BuildFailedException(message);
            }

            Debug.Log("[ConfigValidation] Pre-build validation passed.");
        }
    }
}
