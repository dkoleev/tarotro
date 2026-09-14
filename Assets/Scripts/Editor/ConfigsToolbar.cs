using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace Editor {
    public class PullConfigsToolbarButton : EditorToolbarButton {
        [MainToolbarElement("TarotroPullConfigs", defaultDockPosition = MainToolbarDockPosition.Left)]
        static PullConfigsToolbarButton Create() => new();

        public PullConfigsToolbarButton() {
            text = "Pull Configs";
            tooltip = "Pull all configs from Google Sheets";
            icon = EditorGUIUtility.IconContent("d_Refresh").image as Texture2D;
            clicked += GoogleSheetsHelper.PullAll;
        }
    }

    public class OpenSheetToolbarButton : EditorToolbarButton {
        [MainToolbarElement("TarotroOpenSheet", defaultDockPosition = MainToolbarDockPosition.Left)]
        static OpenSheetToolbarButton Create() => new();

        public OpenSheetToolbarButton() {
            text = "Open Sheet";
            tooltip = "Open Google Spreadsheet in browser";
            icon = EditorGUIUtility.IconContent("d_BuildSettings.Web.Small").image as Texture2D;
            clicked += GoogleSheetsHelper.OpenSpreadsheet;
        }
    }
}
