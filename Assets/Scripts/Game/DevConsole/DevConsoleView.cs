using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Game.DevConsole {
    public class DevConsoleView : MonoBehaviour {
        private DevConsole _console;

        private GameObject _panel;
        private TMP_InputField _inputField;
        private TextMeshProUGUI _logText;
        private ScrollRect _scrollRect;

        private bool _isOpen;
        private int _historyIndex = -1;
        private readonly List<string> _commandHistory = new();

        public void Initialize(DevConsole console) {
            _console = console;
            _console.EntryAdded += OnEntryAdded;
            BuildUI();
            _panel.SetActive(false);
        }

        private void Update() {
            var keyboard = Keyboard.current;
            if (keyboard == null) return;

            if (keyboard.backquoteKey.wasPressedThisFrame) {
                Toggle();
                return;
            }

            if (!_isOpen) return;

            if (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame) {
                SubmitInput();
            }

            if (keyboard.upArrowKey.wasPressedThisFrame) {
                NavigateHistory(1);
            } else if (keyboard.downArrowKey.wasPressedThisFrame) {
                NavigateHistory(-1);
            }

            if (keyboard.escapeKey.wasPressedThisFrame) {
                Close();
            }
        }

        private void OnDestroy() {
            if (_console != null)
                _console.EntryAdded -= OnEntryAdded;
        }

        public void Toggle() {
            if (_isOpen) Close();
            else Open();
        }

        private void Open() {
            _isOpen = true;
            _panel.SetActive(true);
            _inputField.ActivateInputField();
            _inputField.Select();
            RefreshLog();
        }

        private void Close() {
            _isOpen = false;
            _panel.SetActive(false);
        }

        private void SubmitInput() {
            var text = _inputField.text;
            if (string.IsNullOrWhiteSpace(text)) return;

            _commandHistory.Insert(0, text);
            if (_commandHistory.Count > 50)
                _commandHistory.RemoveAt(_commandHistory.Count - 1);
            _historyIndex = -1;

            _console.ExecuteCommand(text);
            _inputField.text = string.Empty;
            _inputField.ActivateInputField();
        }

        private void NavigateHistory(int direction) {
            if (_commandHistory.Count == 0) return;

            _historyIndex += direction;
            _historyIndex = Mathf.Clamp(_historyIndex, 0, _commandHistory.Count - 1);

            _inputField.text = _commandHistory[_historyIndex];
            _inputField.caretPosition = _inputField.text.Length;
        }

        private void OnEntryAdded(DevConsoleEntry entry) {
            if (!_isOpen) return;
            AppendEntry(entry);
            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void RefreshLog() {
            _logText.text = string.Empty;
            var sb = new StringBuilder();
            foreach (var entry in _console.Entries) {
                sb.AppendLine(FormatEntry(entry));
            }
            _logText.text = sb.ToString();

            Canvas.ForceUpdateCanvases();
            _scrollRect.verticalNormalizedPosition = 0f;
        }

        private void AppendEntry(DevConsoleEntry entry) {
            _logText.text += FormatEntry(entry) + "\n";
        }

        private static string FormatEntry(DevConsoleEntry entry) {
            var color = entry.Type switch {
                DevConsoleEntryType.Error => "#F44336",
                DevConsoleEntryType.Command => "#64B5F6",
                _ => "#FFFFFF"
            };

            return $"<color={color}>{entry.Message}</color>";
        }

        private void BuildUI() {
            var canvasGo = new GameObject("DevConsoleCanvas");
            canvasGo.transform.SetParent(transform);
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 9999;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
            canvasGo.AddComponent<GraphicRaycaster>();

            if (FindFirstObjectByType<EventSystem>() == null) {
                var esGo = new GameObject("EventSystem");
                esGo.transform.SetParent(canvasGo.transform);
                esGo.AddComponent<EventSystem>();
                esGo.AddComponent<InputSystemUIInputModule>();
            }

            _panel = new GameObject("ConsolePanel");
            _panel.transform.SetParent(canvasGo.transform, false);
            var panelRect = _panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0f, 0.0f);
            panelRect.anchorMax = new Vector2(1f, 1.0f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            var panelImg = _panel.AddComponent<Image>();
            panelImg.color = new Color(0.05f, 0.05f, 0.1f, 0.92f);

            var layoutGroup = _panel.AddComponent<VerticalLayoutGroup>();
            layoutGroup.padding = new RectOffset(8, 8, 8, 8);
            layoutGroup.spacing = 4;
            layoutGroup.childForceExpandWidth = true;
            layoutGroup.childForceExpandHeight = false;
            layoutGroup.childControlWidth = true;
            layoutGroup.childControlHeight = true;

            BuildScrollView();
            BuildInputField();
        }

        private void BuildScrollView() {
            var scrollGo = new GameObject("ScrollView");
            scrollGo.transform.SetParent(_panel.transform, false);
            var scrollLayout = scrollGo.AddComponent<LayoutElement>();
            scrollLayout.flexibleHeight = 1f;

            _scrollRect = scrollGo.AddComponent<ScrollRect>();
            _scrollRect.horizontal = false;
            _scrollRect.movementType = ScrollRect.MovementType.Clamped;
            _scrollRect.scrollSensitivity = 30f;

            var scrollImg = scrollGo.AddComponent<Image>();
            scrollImg.color = new Color(0f, 0f, 0f, 0.3f);
            var scrollMask = scrollGo.AddComponent<Mask>();
            scrollMask.showMaskGraphic = true;

            var contentGo = new GameObject("Content");
            contentGo.transform.SetParent(scrollGo.transform, false);
            var contentRect = contentGo.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0, 1);
            contentRect.anchorMax = new Vector2(1, 1);
            contentRect.pivot = new Vector2(0, 1);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var contentFitter = contentGo.AddComponent<ContentSizeFitter>();
            contentFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            contentFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            var textGo = new GameObject("LogText");
            textGo.transform.SetParent(contentGo.transform, false);
            var textRect = textGo.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(4, 0);
            textRect.offsetMax = new Vector2(-4, 0);

            _logText = textGo.AddComponent<TextMeshProUGUI>();
            _logText.fontSize = 24;
            _logText.font = TMP_Settings.defaultFontAsset;
            _logText.color = Color.white;
            _logText.richText = true;
            _logText.textWrappingMode = TextWrappingModes.Normal;
            _logText.overflowMode = TextOverflowModes.Overflow;

            var textFitter = textGo.AddComponent<ContentSizeFitter>();
            textFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            _scrollRect.viewport = scrollGo.GetComponent<RectTransform>();
            _scrollRect.content = contentRect;
        }

        private void BuildInputField() {
            var inputGo = new GameObject("InputField");
            inputGo.transform.SetParent(_panel.transform, false);
            var inputLayout = inputGo.AddComponent<LayoutElement>();
            inputLayout.minHeight = 30;
            inputLayout.preferredHeight = 30;

            var inputBg = inputGo.AddComponent<Image>();
            inputBg.color = new Color(0.1f, 0.1f, 0.15f, 1f);

            _inputField = inputGo.AddComponent<TMP_InputField>();
            _inputField.onFocusSelectAll = false;

            var textArea = new GameObject("Text Area");
            textArea.transform.SetParent(inputGo.transform, false);
            var textAreaRect = textArea.AddComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(8, 2);
            textAreaRect.offsetMax = new Vector2(-8, -2);
            textArea.AddComponent<RectMask2D>();

            var inputText = new GameObject("Text");
            inputText.transform.SetParent(textArea.transform, false);
            var inputTextRect = inputText.AddComponent<RectTransform>();
            inputTextRect.anchorMin = Vector2.zero;
            inputTextRect.anchorMax = Vector2.one;
            inputTextRect.offsetMin = Vector2.zero;
            inputTextRect.offsetMax = Vector2.zero;

            var tmp = inputText.AddComponent<TextMeshProUGUI>();
            tmp.fontSize = 20;
            tmp.font = TMP_Settings.defaultFontAsset;
            tmp.color = Color.white;
            tmp.richText = false;

            _inputField.textViewport = textAreaRect;
            _inputField.textComponent = tmp;

            var placeholder = new GameObject("Placeholder");
            placeholder.transform.SetParent(textArea.transform, false);
            var phRect = placeholder.AddComponent<RectTransform>();
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;

            var phText = placeholder.AddComponent<TextMeshProUGUI>();
            phText.fontSize = 14;
            phText.font = TMP_Settings.defaultFontAsset;
            phText.color = new Color(1f, 1f, 1f, 0.3f);
            phText.text = "Type command... (` toggle, Up/Down history, Esc close)";
            phText.fontStyle = FontStyles.Italic;

            _inputField.placeholder = phText;
        }
    }
}
