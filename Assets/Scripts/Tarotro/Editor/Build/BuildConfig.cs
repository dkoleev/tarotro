using UnityEditor.Build.Profile;
using UnityEngine;

namespace Tarotro.Editor.Build {
    [CreateAssetMenu(fileName = "NewBuildConfig", menuName = "Tarotro/Build Config")]
    public class BuildConfig : ScriptableObject {
        [Header("Build Profile")]
        public BuildProfile buildProfile;

        [Header("Output")]
        public string outputFolder = "Builds";
        public string subFolder = "Windows";
        public string executableName = "Tarotro.exe";

        [Header("Pre-Build Steps")]
        public bool validateConfigs = true;
        public bool buildAddressables = true;

        public string OutputPath => $"{outputFolder}/{subFolder}/{executableName}";
    }
}
