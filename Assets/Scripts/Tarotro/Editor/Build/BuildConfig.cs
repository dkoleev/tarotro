using UnityEditor.Build.Profile;
using UnityEngine;

namespace Tarotro.Editor.Build {
    [CreateAssetMenu(fileName = "NewBuildConfig", menuName = "Tarotro/Build Config")]
    public class BuildConfig : ScriptableObject {
        [Header("Build Profile")]
        public BuildProfile buildProfile;

        [Header("Code Variant")]
        [Tooltip("Enables DEBUG define and dev-only code (e.g. DevConsole).")]
        public bool isDevelopment;

        [Tooltip("Additional scripting defines applied for this build variant.")]
        public string[] extraScriptingDefines = {};

        [Header("Output")]
        public string outputFolder = "Builds";
        public string subFolder = "Windows";
        public string executableName = "Tarotro.exe";

        [Header("Pre-Build Steps")]
        public bool validateConfigs = true;
        public bool buildAddressables = true;

        [Header("Post-Build Steps")]
        [Tooltip("Opens the output folder in the file explorer after a successful build.")]
        public bool openFolderAfterBuild = true;

        public string OutputPath => $"{outputFolder}/{subFolder}/{executableName}";
    }
}
