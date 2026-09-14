using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Editor {
    [InitializeOnLoad]
    public static class ConfigsToolbar {
        private const string ContainerId = "tarotro-configs-toolbar";

        static readonly Type ToolbarType =
            typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Toolbar");

        static ConfigsToolbar() {
            EditorApplication.delayCall += AttachToToolbar;
        }

        private static void AttachToToolbar() {
            if (ToolbarType == null) return;

            var toolbars = Resources.FindObjectsOfTypeAll(ToolbarType);
            if (toolbars.Length == 0) {
                EditorApplication.delayCall += AttachToToolbar;
                return;
            }

            var toolbar = toolbars[0] as EditorWindow;
            if (toolbar == null) return;

            var root = toolbar.rootVisualElement;

            if (root.Q(ContainerId) != null) return;

            var zone = root.Q("ToolbarZoneRightAlign");
            if (zone == null) {
                Debug.LogWarning(
                    "[ConfigsToolbar] Could not find toolbar zone. Use Tarotro > Configs menu instead.");
                return;
            }

            var container = new VisualElement { name = ContainerId };
            container.style.flexDirection = FlexDirection.Row;
            container.style.alignItems = Align.Center;

            var pullBtn = new Button(GoogleSheetsHelper.PullAll) {
                text = "Pull Configs",
                tooltip = "Pull all configs from Google Sheets"
            };
            pullBtn.AddToClassList("unity-toolbar-button");

            var openBtn = new Button(GoogleSheetsHelper.OpenSpreadsheet) {
                text = "Open Sheet",
                tooltip = "Open Google Spreadsheet in browser"
            };
            openBtn.AddToClassList("unity-toolbar-button");

            container.Add(pullBtn);
            container.Add(openBtn);
            zone.Add(container);
        }
    }
}
