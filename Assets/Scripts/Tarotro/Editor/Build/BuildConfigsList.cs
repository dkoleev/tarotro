using System.Collections.Generic;
using UnityEngine;

namespace Tarotro.Editor.Build {
    [CreateAssetMenu(fileName = "BuildConfigs", menuName = "Tarotro/Build Configs List")]
    public class BuildConfigsList : ScriptableObject {
        public BuildConfig defaultConfig;
        public BuildConfig developmentConfig;
        public List<BuildConfig> configs = new();
    }
}
