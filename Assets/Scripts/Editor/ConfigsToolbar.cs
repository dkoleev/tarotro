using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor {
    public static class ConfigsToolbar {
        [MainToolbarElement("TarotroPullConfigs", defaultDockPosition = MainToolbarDockPosition.Left)]
        static VisualElement CreatePullButton() {
            var button = new Button(GoogleSheetsHelper.PullAll) {
                text = "Pull Configs",
                tooltip = "Pull all configs from Google Sheets"
            };
            button.style.backgroundImage =
                Background.FromTexture2D(EditorGUIUtility.IconContent("d_Refresh").image as Texture2D);
            return button;
        }

        [MainToolbarElement("TarotroOpenSheet", defaultDockPosition = MainToolbarDockPosition.Left)]
        static VisualElement CreateOpenButton() {
            var button = new Button(GoogleSheetsHelper.OpenSpreadsheet) {
                text = "Open Sheet",
                tooltip = "Open Google Spreadsheet in browser"
            };
            return button;
        }
    }
}
