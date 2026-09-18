using System;
using System.Collections.Generic;
using System.IO;
using MemoryPack;
using Tarotro.Game.Data;
using Tarotro.Game.Data.Save;
using UnityEditor;
using UnityEngine;

namespace Tarotro.Editor {
    public class SaveEditorWindow : EditorWindow {
        private const string SaveFileName = "save.bin";
        private const string BackupSuffix = ".bak";

        private GameSaveData _saveData;
        private Vector2 _scrollPosition;
        private string _savePath;
        private string _backupPath;
        private bool _isDirty;
        private string _statusMessage;
        private MessageType _statusType;

        private bool _battleFoldout = true;
        private bool _currentRoundFoldout = true;
        private bool _enemyFoldout = true;
        private bool _playerFoldout = true;
        private bool _handFoldout;
        private bool _circleFoldout;
        private bool _deckFoldout;
        private bool _cardsFoldout;
        private bool _drawPileFoldout;
        private bool _discardPileFoldout;
        private readonly HashSet<string> _expandedCards = new();

        [MenuItem("Tarotro/Save/Save Editor")]
        private static void ShowWindow() {
            var window = GetWindow<SaveEditorWindow>("Save Editor");
            window.minSize = new Vector2(420, 350);
        }

        [MenuItem("Tarotro/Save/Open Save Location")]
        private static void OpenSaveLocation() {
            var savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            EditorUtility.RevealInFinder(File.Exists(savePath) ? savePath : Application.persistentDataPath);
        }

        private void OnEnable() {
            _savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            _backupPath = _savePath + BackupSuffix;
            LoadSave();
        }

        private void OnGUI() {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

            DrawFileInfo();
            DrawToolbar();

            if (!string.IsNullOrEmpty(_statusMessage)) {
                EditorGUILayout.HelpBox(_statusMessage, _statusType);
            }

            if (_saveData != null) {
                EditorGUILayout.Space(4);
                EditorGUI.BeginChangeCheck();
                DrawGeneralSection();
                DrawBattleSection();
                if (EditorGUI.EndChangeCheck()) {
                    _isDirty = true;
                }
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawFileInfo() {
            EditorGUILayout.LabelField("File Info", EditorStyles.boldLabel);

            using (new EditorGUI.IndentLevelScope()) {
                EditorGUILayout.LabelField("Path", _savePath);

                if (File.Exists(_savePath)) {
                    var info = new FileInfo(_savePath);
                    EditorGUILayout.LabelField("Size", $"{info.Length} bytes");
                    EditorGUILayout.LabelField("Modified", info.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss"));
                } else {
                    EditorGUILayout.LabelField("Status", "No save file");
                }

                if (File.Exists(_backupPath)) {
                    var backupInfo = new FileInfo(_backupPath);
                    EditorGUILayout.LabelField("Backup", $"{backupInfo.Length} bytes ({backupInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss})");
                }
            }
        }

        private void DrawToolbar() {
            EditorGUILayout.Space(4);
            using (new EditorGUILayout.HorizontalScope()) {
                if (GUILayout.Button("Refresh", GUILayout.Width(70))) {
                    LoadSave();
                }

                using (new EditorGUI.DisabledScope(!_isDirty)) {
                    if (GUILayout.Button("Save Changes", GUILayout.Width(100))) {
                        WriteSave();
                    }
                }

                using (new EditorGUI.DisabledScope(_saveData == null)) {
                    if (GUILayout.Button("Delete Save", GUILayout.Width(90))) {
                        if (EditorUtility.DisplayDialog("Delete Save", "Delete save file and backup?", "Delete", "Cancel")) {
                            DeleteSave();
                        }
                    }
                }

                if (GUILayout.Button("Open Folder", GUILayout.Width(85))) {
                    OpenSaveLocation();
                }
            }

            if (_isDirty) {
                EditorGUILayout.HelpBox("Unsaved changes", MessageType.Warning);
            }
        }

        private void DrawGeneralSection() {
            EditorGUILayout.LabelField("General", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope()) {
                using (new EditorGUI.DisabledScope(true)) {
                    EditorGUILayout.IntField("Version", _saveData.Version);
                }
                _saveData.Score = EditorGUILayout.IntField("Score", _saveData.Score);
            }
        }

        private void DrawBattleSection() {
            if (_saveData.Battle == null) {
                EditorGUILayout.LabelField("Battle", "null");
                return;
            }

            _battleFoldout = EditorGUILayout.Foldout(_battleFoldout, "Battle", true, EditorStyles.foldoutHeader);
            if (!_battleFoldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                _saveData.Battle.CurrentCircleIndex = EditorGUILayout.IntField("Circle Index", _saveData.Battle.CurrentCircleIndex);

                DrawCurrentRound();
                DrawEnemy();
                DrawCircleRounds();
                DrawPlayer();
            }
        }

        private void DrawCurrentRound() {
            if (_saveData.Battle.CurrentRound == null) {
                EditorGUILayout.LabelField("Current Round", "null");
                return;
            }

            _currentRoundFoldout = EditorGUILayout.Foldout(_currentRoundFoldout, "Current Round", true);
            if (!_currentRoundFoldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                DrawRoundFields(_saveData.Battle.CurrentRound);
            }
        }

        private void DrawEnemy() {
            if (_saveData.Battle.CurrentEnemy == null) {
                EditorGUILayout.LabelField("Current Enemy", "none");
                return;
            }

            _enemyFoldout = EditorGUILayout.Foldout(_enemyFoldout, "Current Enemy", true);
            if (!_enemyFoldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                _saveData.Battle.CurrentEnemy.EnemyId = EditorGUILayout.TextField("Enemy Id", _saveData.Battle.CurrentEnemy.EnemyId);
                _saveData.Battle.CurrentEnemy.CurrentHealth = EditorGUILayout.IntField("Health", _saveData.Battle.CurrentEnemy.CurrentHealth);
            }
        }

        private void DrawCircleRounds() {
            var rounds = _saveData.Battle.CurrentCircle;
            if (rounds == null || rounds.Count == 0) {
                EditorGUILayout.LabelField("Circle Rounds", "empty");
                return;
            }

            _circleFoldout = EditorGUILayout.Foldout(_circleFoldout, $"Circle Rounds ({rounds.Count})", true);
            if (!_circleFoldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                for (var i = 0; i < rounds.Count; i++) {
                    var key = $"round_{i}";
                    var expanded = _expandedCards.Contains(key);
                    var label = $"Round {i}: {rounds[i].Circle} / {rounds[i].EnemyType} (HP: {rounds[i].TargetScore})";
                    var newExpanded = EditorGUILayout.Foldout(expanded, label, true);

                    if (newExpanded != expanded) {
                        if (newExpanded) _expandedCards.Add(key);
                        else _expandedCards.Remove(key);
                    }

                    if (!newExpanded) continue;

                    using (new EditorGUI.IndentLevelScope()) {
                        DrawRoundFields(rounds[i]);
                    }
                }
            }
        }

        private void DrawPlayer() {
            if (_saveData.Battle.Player == null) {
                EditorGUILayout.LabelField("Player", "null");
                return;
            }

            _playerFoldout = EditorGUILayout.Foldout(_playerFoldout, "Player", true, EditorStyles.foldoutHeader);
            if (!_playerFoldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                DrawCardList("Hand", ref _handFoldout, _saveData.Battle.Player.Hand, "hand");

                if (_saveData.Battle.Player.Deck != null) {
                    _deckFoldout = EditorGUILayout.Foldout(_deckFoldout, "Deck", true);
                    if (_deckFoldout) {
                        using (new EditorGUI.IndentLevelScope()) {
                            DrawCardList("Cards", ref _cardsFoldout, _saveData.Battle.Player.Deck.Cards, "cards");
                            DrawCardList("Draw Pile", ref _drawPileFoldout, _saveData.Battle.Player.Deck.DrawPile, "draw");
                            DrawCardList("Discard Pile", ref _discardPileFoldout, _saveData.Battle.Player.Deck.DiscardPile, "discard");
                        }
                    }
                }
            }
        }

        private void DrawCardList(string label, ref bool foldout, List<CardSaveData> cards, string keyPrefix) {
            if (cards == null || cards.Count == 0) {
                EditorGUILayout.LabelField(label, "empty");
                return;
            }

            foldout = EditorGUILayout.Foldout(foldout, $"{label} ({cards.Count})", true);
            if (!foldout) return;

            using (new EditorGUI.IndentLevelScope()) {
                for (var i = 0; i < cards.Count; i++) {
                    var card = cards[i];
                    var key = $"{keyPrefix}_{i}";
                    var expanded = _expandedCards.Contains(key);
                    var cardLabel = $"[{i}] {card.Name ?? "?"} (Dmg: {card.Damage}, Cost: {card.Cost})";
                    var newExpanded = EditorGUILayout.Foldout(expanded, cardLabel, true);

                    if (newExpanded != expanded) {
                        if (newExpanded) _expandedCards.Add(key);
                        else _expandedCards.Remove(key);
                    }

                    if (!newExpanded) continue;

                    using (new EditorGUI.IndentLevelScope()) {
                        card.Id = EditorGUILayout.TextField("Id", card.Id);
                        card.Name = EditorGUILayout.TextField("Name", card.Name);
                        card.Damage = EditorGUILayout.IntField("Damage", card.Damage);
                        card.Cost = EditorGUILayout.IntField("Cost", card.Cost);
                        card.Description = EditorGUILayout.TextField("Description", card.Description);
                    }
                }
            }
        }

        private static void DrawRoundFields(FightRoundSaveData round) {
            round.Circle = (CircleType)EditorGUILayout.EnumPopup("Circle", round.Circle);
            round.CircleStep = EditorGUILayout.IntField("Circle Step", round.CircleStep);
            round.EnemyType = (EnemyType)EditorGUILayout.EnumPopup("Enemy Type", round.EnemyType);
            round.TargetScore = EditorGUILayout.IntField("Target Score", round.TargetScore);
            round.EnemyId = EditorGUILayout.TextField("Enemy Id", round.EnemyId);
        }

        private void LoadSave() {
            _isDirty = false;
            _saveData = null;
            _statusMessage = null;

            var path = File.Exists(_savePath) ? _savePath : (File.Exists(_backupPath) ? _backupPath : null);
            if (path == null) {
                _statusMessage = "No save file found.";
                _statusType = MessageType.Info;
                Repaint();
                return;
            }

            try {
                var bytes = File.ReadAllBytes(path);
                _saveData = MemoryPackSerializer.Deserialize<GameSaveData>(bytes);

                if (_saveData == null) {
                    _statusMessage = "Failed to deserialize save data.";
                    _statusType = MessageType.Error;
                } else {
                    var source = path == _backupPath ? " (from backup)" : "";
                    _statusMessage = $"Loaded successfully{source}.";
                    _statusType = MessageType.Info;
                }
            } catch (Exception ex) {
                _statusMessage = $"Load error: {ex.Message}";
                _statusType = MessageType.Error;
            }

            Repaint();
        }

        private void WriteSave() {
            if (_saveData == null) return;

            try {
                var bytes = MemoryPackSerializer.Serialize(_saveData);
                var tempPath = _savePath + ".tmp";

                File.WriteAllBytes(tempPath, bytes);

                if (File.Exists(_savePath)) {
                    File.Replace(tempPath, _savePath, _backupPath);
                } else {
                    File.Move(tempPath, _savePath);
                }

                _isDirty = false;
                _statusMessage = $"Saved ({bytes.Length} bytes).";
                _statusType = MessageType.Info;
            } catch (Exception ex) {
                _statusMessage = $"Save error: {ex.Message}";
                _statusType = MessageType.Error;
            }

            Repaint();
        }

        private void DeleteSave() {
            if (File.Exists(_savePath)) File.Delete(_savePath);
            if (File.Exists(_backupPath)) File.Delete(_backupPath);

            _saveData = null;
            _isDirty = false;
            _statusMessage = "Save files deleted.";
            _statusType = MessageType.Info;
            Repaint();
        }
    }
}
