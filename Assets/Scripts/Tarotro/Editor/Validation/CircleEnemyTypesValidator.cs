using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Tarotro.Game.Data;
using UnityEngine;

namespace Tarotro.Editor.Validation {
    public class CircleEnemyTypesValidator : IConfigValidator {
        private const string CirclesPath = "Assets/Configs/circles.json";
        private const string EnemiesPath = "Assets/Configs/enemies.json";

        public string Name => "Circle Enemy Types Coverage";

        public ConfigValidationResult Validate() {
            var result = new ConfigValidationResult();

            var circlesAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(CirclesPath);
            if (circlesAsset == null) {
                result.AddError($"Cannot load {CirclesPath}");
                return result;
            }

            var enemiesAsset = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(EnemiesPath);
            if (enemiesAsset == null) {
                result.AddError($"Cannot load {EnemiesPath}");
                return result;
            }

            var circles = JsonConvert.DeserializeObject<Dictionary<string, CircleData>>(circlesAsset.text);
            var enemies = JsonConvert.DeserializeObject<Dictionary<string, EnemyData>>(enemiesAsset.text);

            foreach (var (circleName, circle) in circles) {
                if (circle.steps == null || circle.steps.Count == 0) {
                    result.AddWarning($"Circle '{circleName}' has no steps defined.");
                    continue;
                }

                if (circle.enemies == null || circle.enemies.Count == 0) {
                    var missingTypes = string.Join(", ", circle.steps);
                    result.AddError($"Circle '{circleName}' has no enemies but requires types: [{missingTypes}]");
                    continue;
                }

                var coveredTypes = new HashSet<EnemyType>();
                foreach (var enemyId in circle.enemies) {
                    if (enemies.TryGetValue(enemyId, out var enemyData)) {
                        coveredTypes.Add(enemyData.type);
                    } else {
                        result.AddError($"Circle '{circleName}' references unknown enemy '{enemyId}'.");
                    }
                }

                foreach (var requiredType in circle.steps) {
                    if (!coveredTypes.Contains(requiredType)) {
                        var availableOfType = enemies
                            .Where(e => e.Value.type == requiredType)
                            .Select(e => e.Key);
                        var suggestions = string.Join(", ", availableOfType);

                        result.AddError(
                            $"Circle '{circleName}' is missing enemies of type '{requiredType}'. " +
                            $"Available: [{suggestions}]");
                    }
                }
            }

            return result;
        }
    }
}
