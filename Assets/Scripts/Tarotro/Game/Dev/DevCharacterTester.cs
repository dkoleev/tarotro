using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using Tarotro.Game.Data;
using Tarotro.Game.View;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Tarotro.Game.Dev {
    public class DevCharacterTester : MonoBehaviour {
        [SerializeField] private float spawnSpacing = 1.5f;
        [SerializeField] private float cameraPanSpeed = 3f;
        [SerializeField] private int uiFontSize = 20;
        [SerializeField] private float panelWidth = 340f;
        [SerializeField] private float scrollHeight = 300f;

        private Dictionary<string, CharacterData> _characters;
        private bool _configsLoaded;
        private string _loadingStatus = "Loading configs...";

        private readonly List<SpawnedCharacter> _spawned = new();
        private Vector2 _scrollPos;
        private int _selectedIndex = -1;
        private Camera _cam;
        private float _nextSpawnX;

        private GUIStyle _labelStyle;
        private GUIStyle _boldLabelStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _boldButtonStyle;

        private class SpawnedCharacter {
            public string Id;
            public GameObject Go;
            public ICharacterView View;
            public AsyncOperationHandle<GameObject> Handle;
            public CancellationTokenSource Cts;
        }

        private async void Start() {
            _cam = Camera.main;
            await LoadConfigs();
        }

        private void Update() {
            if (_cam == null) return;

            var move = Vector3.zero;
            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) move.y += 1;
            if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) move.y -= 1;
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) move.x -= 1;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) move.x += 1;

            if (move != Vector3.zero)
                _cam.transform.position += move.normalized * (cameraPanSpeed * Time.deltaTime);

            var scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) > 0.001f)
                _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize - scroll * 0.5f, 0.5f, 10f);
        }

        private void OnDestroy() {
            ClearAll();
        }

        private async UniTask LoadConfigs() {
            try {
                var charHandle = Addressables.LoadAssetAsync<TextAsset>("Configs/characters.json");
                var charAsset = await charHandle.ToUniTask();
                _characters = JsonConvert.DeserializeObject<Dictionary<string, CharacterData>>(charAsset.text);
                Addressables.Release(charHandle);

                _configsLoaded = true;
                _loadingStatus = $"Loaded {_characters.Count} characters";
            }
            catch (Exception e) {
                _loadingStatus = $"Failed to load configs: {e.Message}";
            }
        }

        private async UniTask SpawnCharacter(string id) {
            if (!_characters.TryGetValue(id, out var data)) return;

            var handle = Addressables.InstantiateAsync(data.prefabPath);
            var go = await handle.ToUniTask();

            go.transform.position = new Vector3(_nextSpawnX, 0, 0);
            _nextSpawnX += spawnSpacing;

            var view = go.GetComponent<ICharacterView>();
            view?.SetHealth(100);
            view.SetInterfaceActive(false);

            var spawned = new SpawnedCharacter {
                Id = id,
                Go = go,
                View = view,
                Handle = handle,
                Cts = new CancellationTokenSource()
            };
            _spawned.Add(spawned);
            _selectedIndex = _spawned.Count - 1;
        }

        private void RemoveCharacter(int index) {
            var c = _spawned[index];
            c.Cts.Cancel();
            c.Cts.Dispose();
            if (c.Handle.IsValid()) Addressables.ReleaseInstance(c.Handle);
            _spawned.RemoveAt(index);
            if (_selectedIndex >= _spawned.Count) _selectedIndex = _spawned.Count - 1;
        }

        private void ClearAll() {
            for (var i = _spawned.Count - 1; i >= 0; i--) {
                var c = _spawned[i];
                c.Cts.Cancel();
                c.Cts.Dispose();
                if (c.Handle.IsValid()) Addressables.ReleaseInstance(c.Handle);
            }
            _spawned.Clear();
            _selectedIndex = -1;
            _nextSpawnX = 0;
        }

        private void SpawnAll() {
            SpawnAllAsync().Forget();
        }

        private async UniTask SpawnAllAsync() {
            foreach (var kv in _characters) {
                await SpawnCharacter(kv.Key);
            }
        }

        private void InitStyles() {
            if (_labelStyle != null) return;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = uiFontSize };
            _boldLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = uiFontSize, richText = true, fontStyle = FontStyle.Bold };
            _buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = uiFontSize };
            _boldButtonStyle = new GUIStyle(GUI.skin.button) { fontSize = uiFontSize, fontStyle = FontStyle.Bold };
        }

        private void OnGUI() {
            InitStyles();

            var panelHeight = Screen.height;

            GUI.Box(new Rect(0, 0, panelWidth, panelHeight), "");

            GUILayout.BeginArea(new Rect(8, 8, panelWidth - 16, panelHeight - 16));

            GUILayout.Label("<b>Character Tester</b>", _boldLabelStyle);
            GUILayout.Space(4);
            GUILayout.Label(_loadingStatus, _labelStyle);
            GUILayout.Space(4);

            if (_cam != null) {
                GUILayout.Label($"Camera: zoom={_cam.orthographicSize:F1}  WASD=pan  Scroll=zoom", _labelStyle);
            }
            GUILayout.Space(8);

            if (_configsLoaded && _characters != null) {
                GUILayout.Label("<b>Spawn Character:</b>", _boldLabelStyle);

                _scrollPos = GUILayout.BeginScrollView(_scrollPos, GUILayout.Height(scrollHeight));
                foreach (var kv in _characters) {
                    if (GUILayout.Button(kv.Key, _buttonStyle)) {
                        SpawnCharacter(kv.Key).Forget();
                    }
                }
                GUILayout.EndScrollView();

                GUILayout.Space(4);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Spawn All", _buttonStyle)) SpawnAll();
                if (GUILayout.Button("Clear All", _buttonStyle)) ClearAll();
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(12);

            if (_spawned.Count > 0) {
                GUILayout.Label($"<b>Spawned ({_spawned.Count}):</b>", _boldLabelStyle);

                for (var i = 0; i < _spawned.Count; i++) {
                    var isSelected = i == _selectedIndex;
                    var style = isSelected ? _boldButtonStyle : _buttonStyle;

                    GUILayout.BeginHorizontal();
                    if (GUILayout.Button(_spawned[i].Id, style, GUILayout.ExpandWidth(true))) {
                        _selectedIndex = i;
                    }
                    if (GUILayout.Button("X", _buttonStyle, GUILayout.Width(30))) {
                        RemoveCharacter(i);
                        i--;
                    }
                    GUILayout.EndHorizontal();
                }

                if (_selectedIndex >= 0 && _selectedIndex < _spawned.Count) {
                    GUILayout.Space(8);
                    DrawSelectedControls(_spawned[_selectedIndex]);
                }
            }

            GUILayout.EndArea();
        }

        private void DrawSelectedControls(SpawnedCharacter c) {
            if (c.Go == null) return;

            GUILayout.Label($"<b>Selected: {c.Id}</b>", _boldLabelStyle);

            GUILayout.Label("Animations:", _labelStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Idle", _buttonStyle)) PlayAnimation(c, v => v.PlayIdleAnimation(c.Cts.Token));
            if (GUILayout.Button("Attack", _buttonStyle)) PlayAnimation(c, v => v.PlayAttackAnimation(c.Cts.Token));
            if (GUILayout.Button("Death", _buttonStyle)) PlayAnimation(c, v => v.PlayDeathAnimation(c.Cts.Token));
            if (GUILayout.Button("Emote", _buttonStyle)) PlayAnimation(c, v => v.PlayEmoteAnimation(c.Cts.Token));
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            var t = c.Go.transform;
            var pos = t.position;

            GUILayout.Label($"Position: ({pos.x:F2}, {pos.y:F2})", _labelStyle);
            GUILayout.BeginHorizontal();
            GUILayout.Label("X:", _labelStyle, GUILayout.Width(24));
            var newX = GUILayout.HorizontalSlider(pos.x, -5f, 5f);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("Y:", _labelStyle, GUILayout.Width(24));
            var newY = GUILayout.HorizontalSlider(pos.y, -3f, 3f);
            GUILayout.EndHorizontal();

            if (Math.Abs(newX - pos.x) > 0.001f || Math.Abs(newY - pos.y) > 0.001f)
                t.position = new Vector3(newX, newY, pos.z);

            GUILayout.Space(4);
            var scale = t.localScale;
            GUILayout.BeginHorizontal();
            GUILayout.Label("Scale:", _labelStyle, GUILayout.Width(52));
            var newScale = GUILayout.HorizontalSlider(scale.x, 0.1f, 5f);
            GUILayout.EndHorizontal();

            if (Math.Abs(newScale - scale.x) > 0.001f)
                t.localScale = new Vector3(newScale, Mathf.Abs(newScale), scale.z);

            GUILayout.Space(2);
            if (GUILayout.Button(scale.x < 0 ? "Flip: ON" : "Flip: OFF", _buttonStyle)) {
                t.localScale = new Vector3(-scale.x, scale.y, scale.z);
            }
        }

        private static void PlayAnimation(SpawnedCharacter c, Func<ICharacterView, UniTask> play) {
            if (c.View == null) return;
            play(c.View).Forget();
        }
    }
}