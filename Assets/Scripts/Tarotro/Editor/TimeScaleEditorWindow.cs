using UnityEditor;
using UnityEngine;

namespace Tarotro.Editor {
    public class TimeScaleEditorWindow : EditorWindow {
        private static readonly float[] Presets = { 0f, 0.25f, 0.5f, 1f, 2f, 5f, 10f };

        private float _timeScale = 1f;
        private bool _restoreOnExitPlay = true;

        [MenuItem("Tarotro/Time Scale")]
        private static void ShowWindow() {
            var window = GetWindow<TimeScaleEditorWindow>("Time Scale");
            window.minSize = new Vector2(280, 130);
        }

        private void OnEnable() {
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            _timeScale = Time.timeScale;
        }

        private void OnDisable() {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void Update() {
            if (EditorApplication.isPlaying && !Mathf.Approximately(Time.timeScale, _timeScale)) {
                _timeScale = Time.timeScale;
                Repaint();
            }
        }

        private void OnGUI() {
            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Current Time Scale", EditorStyles.boldLabel, GUILayout.Width(130));
                EditorGUILayout.LabelField(Time.timeScale.ToString("F2"), GUILayout.Width(50));
            }

            EditorGUILayout.Space(4);

            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.LabelField("Scale", GUILayout.Width(40));
                var newScale = EditorGUILayout.Slider(_timeScale, 0f, 10f);
                if (!Mathf.Approximately(newScale, _timeScale)) {
                    _timeScale = newScale;
                    ApplyTimeScale();
                }
            }

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Presets", EditorStyles.miniLabel);

            using (new EditorGUILayout.HorizontalScope()) {
                foreach (var preset in Presets) {
                    var label = preset == 0f ? "Pause" : $"{preset}x";
                    if (GUILayout.Button(label, GUILayout.MinWidth(40))) {
                        _timeScale = preset;
                        ApplyTimeScale();
                    }
                }
            }

            EditorGUILayout.Space(4);
            _restoreOnExitPlay = EditorGUILayout.Toggle("Restore 1x on Stop", _restoreOnExitPlay);

            if (!EditorApplication.isPlaying) {
                EditorGUILayout.HelpBox("Enter Play Mode to change Time Scale.", MessageType.Info);
            }
        }

        private void ApplyTimeScale() {
            if (EditorApplication.isPlaying) {
                Time.timeScale = _timeScale;
            }

            Repaint();
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state) {
            if (state == PlayModeStateChange.EnteredPlayMode) {
                Time.timeScale = _timeScale;
                Repaint();
            } else if (state == PlayModeStateChange.ExitingPlayMode && _restoreOnExitPlay) {
                _timeScale = 1f;
                Time.timeScale = 1f;
                Repaint();
            }
        }
    }
}
