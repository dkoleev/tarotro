using System;
using UnityEditor;

namespace Tarotro.Editor.Build {
    [Serializable]
    public class BuildConfig {
        public string OutputFolder { get; set; } = "Builds";
        public string ExecutableName { get; set; } = "Tarotro.exe";
        public BuildTarget Target { get; set; } = BuildTarget.StandaloneWindows64;
        public bool Development { get; set; }
        public bool ValidateConfigs { get; set; } = true;
        public bool BuildAddressables { get; set; } = true;

        public string OutputPath => $"{OutputFolder}/Windows/{ExecutableName}";

        public BuildOptions GetBuildOptions() {
            var options = BuildOptions.None;
            if (Development)
                options |= BuildOptions.Development | BuildOptions.AllowDebugging;
            return options;
        }
    }
}
