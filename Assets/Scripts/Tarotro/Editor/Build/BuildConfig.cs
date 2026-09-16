using UnityEditor;
using UnityEngine;

namespace Tarotro.Editor.Build {
    [CreateAssetMenu(fileName = "NewBuildConfig", menuName = "Tarotro/Build Config")]
    public class BuildConfig : ScriptableObject {
        [Header("Output")]
        public string outputFolder = "Builds";
        public string executableName = "Tarotro.exe";
        public string subFolder = "Windows";

        [Header("Platform")]
        public BuildTarget target = BuildTarget.StandaloneWindows64;

        [Header("Options")]
        public bool development;
        public bool validateConfigs = true;
        public bool buildAddressables = true;

        public string OutputPath => $"{outputFolder}/{subFolder}/{executableName}";

        public BuildOptions GetBuildOptions() {
            var options = BuildOptions.None;
            if (development)
                options |= BuildOptions.Development | BuildOptions.AllowDebugging;
            return options;
        }
    }
}
