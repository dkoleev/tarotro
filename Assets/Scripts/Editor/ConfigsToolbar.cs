using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor {
    [EditorToolbarElement(Id, typeof(SceneView))]
    public class PullConfigsButton : EditorToolbarButton {
        public const string Id = "Tarotro/PullConfigs";

        public PullConfigsButton() {
            text = "Pull Configs";
            tooltip = "Pull all configs from Google Sheets";
            icon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            clicked += GoogleSheetsHelper.PullAll;
        }
    }

    [EditorToolbarElement(Id, typeof(SceneView))]
    public class OpenSpreadsheetButton : EditorToolbarButton {
        public const string Id = "Tarotro/OpenSpreadsheet";

        public OpenSpreadsheetButton() {
            text = "Open Sheet";
            tooltip = "Open Google Spreadsheet in browser";
            icon = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small").image as Texture2D;
            clicked += GoogleSheetsHelper.OpenSpreadsheet;
        }
    }

    [Overlay(typeof(SceneView), OverlayId, "Configs")]
    [Icon("d_Refresh")]
    public class ConfigsToolbarOverlay : ToolbarOverlay {
        private const string OverlayId = "tarotro-configs-toolbar";

        ConfigsToolbarOverlay() : base(
            PullConfigsButton.Id,
            OpenSpreadsheetButton.Id
        ) { }
    }
}
