using System.Collections.Generic;

namespace Tarotro.Editor.Validation {
    public class ConfigValidationResult {
        public bool IsValid => Errors.Count == 0;
        public List<string> Errors { get; } = new();
        public List<string> Warnings { get; } = new();

        public void AddError(string message) {
            Errors.Add(message);
        }

        public void AddWarning(string message) {
            Warnings.Add(message);
        }

        public void Merge(ConfigValidationResult other) {
            Errors.AddRange(other.Errors);
            Warnings.AddRange(other.Warnings);
        }
    }
}
