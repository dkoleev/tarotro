using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Tarotro.Editor {
    public class SpritesheetPivotEditor : EditorWindow {
        private SpriteAlignment _alignment = SpriteAlignment.Center;
        private Vector2 _customPivot = new(0.5f, 0.5f);
        private Vector2 _scrollPosition;
        private List<TextureImporter> _selectedImporters = new();

        [MenuItem("Tarotro/Spritesheet Pivot Editor", priority = 200)]
        private static void ShowWindow() {
            GetWindow<SpritesheetPivotEditor>("Spritesheet Pivot Editor");
        }

        private void OnSelectionChange() {
            RefreshSelection();
            Repaint();
        }

        private void OnEnable() {
            RefreshSelection();
        }

        private void RefreshSelection() {
            _selectedImporters.Clear();

            foreach (var obj in Selection.objects) {
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path))
                    continue;

                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null || importer.spriteImportMode != SpriteImportMode.Multiple)
                    continue;

                _selectedImporters.Add(importer);
            }
        }

        private void OnGUI() {
            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Spritesheet Pivot Editor", EditorStyles.boldLabel);
            EditorGUILayout.Space(4);

            if (_selectedImporters.Count == 0) {
                EditorGUILayout.HelpBox(
                    "Select one or more spritesheets (Sprite Mode: Multiple) in the Project window.",
                    MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Selected spritesheets: {_selectedImporters.Count}");
            EditorGUILayout.Space(4);

            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.MaxHeight(120));
            foreach (var importer in _selectedImporters) {
                var spriteCount = importer.spritesheet.Length;
                EditorGUILayout.LabelField($"  {importer.assetPath}  ({spriteCount} sprites)");
            }
            EditorGUILayout.EndScrollView();

            EditorGUILayout.Space(8);
            EditorGUILayout.LabelField("Pivot Settings", EditorStyles.boldLabel);

            _alignment = (SpriteAlignment)EditorGUILayout.EnumPopup("Pivot", _alignment);

            if (_alignment == SpriteAlignment.Custom) {
                _customPivot = EditorGUILayout.Vector2Field("Custom Pivot", _customPivot);
            }

            EditorGUILayout.Space(12);

            if (GUILayout.Button("Apply Pivot to All Sprites", GUILayout.Height(30))) {
                ApplyPivot();
            }
        }

        private void ApplyPivot() {
            var totalSprites = 0;

            foreach (var importer in _selectedImporters) {
                var spritesheet = importer.spritesheet;
                if (spritesheet.Length == 0)
                    continue;

                for (var i = 0; i < spritesheet.Length; i++) {
                    spritesheet[i].alignment = (int)_alignment;
                    if (_alignment == SpriteAlignment.Custom) {
                        spritesheet[i].pivot = _customPivot;
                    }
                }

                importer.spritesheet = spritesheet;
                EditorUtility.SetDirty(importer);
                importer.SaveAndReimport();
                totalSprites += spritesheet.Length;
            }

            Debug.Log($"[SpritesheetPivotEditor] Updated pivot for {totalSprites} sprites across {_selectedImporters.Count} spritesheet(s).");
        }
    }
}
