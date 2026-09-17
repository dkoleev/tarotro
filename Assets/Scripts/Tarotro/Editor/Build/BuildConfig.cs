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

        [Header("Version")]
        [Tooltip("Auto-increments the last segment of bundleVersion on each successful build.")]
        public bool autoIncrementBuildNumber;

        [Header("Pre-Build Steps")]
        public bool validateConfigs = true;
        public bool buildAddressables = true;
        [Tooltip("Deletes the output folder before building to prevent stale files from previous builds.")]
        public bool cleanBeforeBuild;

        [Header("Post-Build Steps")]
        [Tooltip("Opens the output folder in the file explorer after a successful build.")]
        public bool openFolderAfterBuild = true;
        [Tooltip("Saves all build logs to a timestamped file in the output folder.")]
        public bool saveBuildLog;

        [Header("Steam Upload")]
        [Tooltip("Runs SteamCMD to upload the build to a depot after a successful build.")]
        public bool uploadToSteam;
        [Tooltip("Path to the SteamCMD executable.")]
        public string steamCmdPath = "";
        [Tooltip("Path to the VDF app build script for SteamCMD.")]
        public string steamVdfPath = "";
        [Tooltip("Steam builder account username. Password is handled by SteamCMD credential caching.")]
        public string steamUsername = "";

        public string OutputPath => $"{outputFolder}/{subFolder}/{executableName}";
        public string OutputFolder => $"{outputFolder}/{subFolder}";
    }
}
