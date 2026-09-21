using Tarotro.Game.Logic.Motion;
using UnityEditor;
using UnityEngine;

namespace Tarotro.Editor
{
    [CustomEditor(typeof(CardHandConfig))]
    public sealed class CardHandConfigEditor : UnityEditor.Editor
    {
        private const int PreviewCardCount = 5;

        private static readonly Color HandAreaColor = new Color(0.2f, 0.8f, 0.4f, 0.25f);
        private static readonly Color HandAreaOutline = new Color(0.2f, 0.8f, 0.4f, 0.9f);
        private static readonly Color DeckColor = new Color(0.3f, 0.5f, 1f, 0.4f);
        private static readonly Color DeckOutline = new Color(0.3f, 0.5f, 1f, 0.9f);
        private static readonly Color DiscardColor = new Color(1f, 0.4f, 0.3f, 0.4f);
        private static readonly Color DiscardOutline = new Color(1f, 0.4f, 0.3f, 0.9f);
        private static readonly Color CardPreviewColor = new Color(1f, 0.9f, 0.3f, 0.5f);

        private bool _showPreviewCards = true;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawDefaultInspector();

            EditorGUILayout.Space(10);
            _showPreviewCards = EditorGUILayout.Toggle("Preview Cards in Scene", _showPreviewCards);

            EditorGUILayout.Space(5);
            EditorGUILayout.HelpBox(
                "Green = Hand Area\n" +
                "Blue = Deck Position\n" +
                "Red = Discard Position\n" +
                "Yellow = Card Preview Slots",
                MessageType.Info);

            if (GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
                SceneView.RepaintAll();
            }
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            var config = (CardHandConfig)target;
            bool flip = config.flipY;

            DrawArea("Hand Area", config.handX, config.handY, config.handWidth, config.handHeight,
                HandAreaColor, HandAreaOutline, flip);

            DrawCard("Deck", config.deckX, config.deckY, config.cardWidth, config.cardHeight,
                DeckColor, DeckOutline, flip);

            DrawCard("Discard", config.discardX, config.discardY, config.cardWidth, config.cardHeight,
                DiscardColor, DiscardOutline, flip);

            if (_showPreviewCards)
            {
                DrawCardPreviews(config, flip);
            }

            DrawHandleForDeck(config, flip);
            DrawHandleForDiscard(config, flip);
            DrawHandleForHandArea(config, flip);

            if (GUI.changed)
            {
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawCardPreviews(CardHandConfig config, bool flip)
        {
            int n = PreviewCardCount;
            int maxCards = Mathf.Max(n, config.softLimit);
            float span = Mathf.Max(maxCards - 1, 1);

            for (int k = 0; k < n; k++)
            {
                float i = k + 1;
                float x = config.handX
                          + (config.handWidth - config.cardWidth)
                          * ((i - 1) / span - 0.5f * (n - maxCards) / span);
                float y = config.handY + config.handHeight * 0.5f - config.cardHeight * 0.5f;

                DrawCard(null, x, y, config.cardWidth, config.cardHeight,
                    CardPreviewColor, CardPreviewColor, flip);
            }
        }

        private static void DrawArea(string label, float x, float y, float w, float h,
            Color fill, Color outline, bool flip)
        {
            float cy = y + h * 0.5f;
            if (flip) { cy = -cy; }
            float cx = x + w * 0.5f;

            var center = new Vector3(cx, cy, 0f);
            var size = new Vector3(w, h, 0f);

            Handles.color = fill;
            Handles.DrawSolidRectangleWithOutline(
                GetRectVerts(center, size),
                fill, outline);

            if (!string.IsNullOrEmpty(label))
            {
                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = outline }
                };
                Handles.Label(center + Vector3.up * (h * 0.5f + 0.3f), label, style);
            }
        }

        private static void DrawCard(string label, float x, float y, float w, float h,
            Color fill, Color outline, bool flip)
        {
            float cy = y + h * 0.5f;
            if (flip) { cy = -cy; }
            float cx = x + w * 0.5f;

            var center = new Vector3(cx, cy, 0f);
            var size = new Vector3(w, h, 0f);

            Handles.DrawSolidRectangleWithOutline(
                GetRectVerts(center, size),
                fill, outline);

            if (!string.IsNullOrEmpty(label))
            {
                var style = new GUIStyle(EditorStyles.boldLabel)
                {
                    alignment = TextAnchor.MiddleCenter,
                    normal = { textColor = outline }
                };
                Handles.Label(center, label, style);
            }
        }

        private void DrawHandleForHandArea(CardHandConfig config, bool flip)
        {
            float cy = config.handY + config.handHeight * 0.5f;
            if (flip) { cy = -cy; }
            float cx = config.handX + config.handWidth * 0.5f;

            var pos = new Vector3(cx, cy, 0f);

            EditorGUI.BeginChangeCheck();
            Handles.color = HandAreaOutline;
            var newPos = Handles.FreeMoveHandle(pos, 0.3f, Vector3.one * 0.5f, Handles.RectangleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Hand Area");
                float ny = newPos.y;
                if (flip) { ny = -ny; }
                config.handX = newPos.x - config.handWidth * 0.5f;
                config.handY = ny - config.handHeight * 0.5f;
                EditorUtility.SetDirty(target);
            }

            float right = config.handX + config.handWidth;
            float bottom = config.handY + config.handHeight;
            float rcy = bottom;
            if (flip) { rcy = -rcy; }
            var sizeHandle = new Vector3(right, rcy, 0f);

            EditorGUI.BeginChangeCheck();
            Handles.color = HandAreaOutline;
            var newSize = Handles.FreeMoveHandle(sizeHandle, 0.2f, Vector3.one * 0.25f, Handles.DotHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Resize Hand Area");
                float nsy = newSize.y;
                if (flip) { nsy = -nsy; }
                config.handWidth = Mathf.Max(1f, newSize.x - config.handX);
                config.handHeight = Mathf.Max(0.5f, nsy - config.handY);
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawHandleForDeck(CardHandConfig config, bool flip)
        {
            float cy = config.deckY + config.cardHeight * 0.5f;
            if (flip) { cy = -cy; }
            float cx = config.deckX + config.cardWidth * 0.5f;

            var pos = new Vector3(cx, cy, 0f);

            EditorGUI.BeginChangeCheck();
            Handles.color = DeckOutline;
            var newPos = Handles.FreeMoveHandle(pos, 0.25f, Vector3.one * 0.5f, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Deck Position");
                float ny = newPos.y;
                if (flip) { ny = -ny; }
                config.deckX = newPos.x - config.cardWidth * 0.5f;
                config.deckY = ny - config.cardHeight * 0.5f;
                EditorUtility.SetDirty(target);
            }
        }

        private void DrawHandleForDiscard(CardHandConfig config, bool flip)
        {
            float cy = config.discardY + config.cardHeight * 0.5f;
            if (flip) { cy = -cy; }
            float cx = config.discardX + config.cardWidth * 0.5f;

            var pos = new Vector3(cx, cy, 0f);

            EditorGUI.BeginChangeCheck();
            Handles.color = DiscardOutline;
            var newPos = Handles.FreeMoveHandle(pos, 0.25f, Vector3.one * 0.5f, Handles.CircleHandleCap);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Discard Position");
                float ny = newPos.y;
                if (flip) { ny = -ny; }
                config.discardX = newPos.x - config.cardWidth * 0.5f;
                config.discardY = ny - config.cardHeight * 0.5f;
                EditorUtility.SetDirty(target);
            }
        }

        private static Vector3[] GetRectVerts(Vector3 center, Vector3 size)
        {
            float hw = size.x * 0.5f;
            float hh = size.y * 0.5f;
            return new[]
            {
                center + new Vector3(-hw, -hh, 0f),
                center + new Vector3(hw, -hh, 0f),
                center + new Vector3(hw, hh, 0f),
                center + new Vector3(-hw, hh, 0f)
            };
        }
    }
}
