using UnityEditor;
using UnityEngine;
using Yogi.UniGSC.Editor;

namespace Tarotro.Editor {
    public static class GoogleSheetsMenu {
        [MenuItem("Tarotro/Configs/Pull All Configs", priority = 0)]
        private static void PullAll() {
            GoogleSheetsHelper.PullAll();
        }

        [MenuItem("Tarotro/Configs/Open Spreadsheet in Browser", priority = 1)]
        private static void OpenSpreadsheet() {
            GoogleSheetsHelper.OpenSpreadsheet();
        }

        [MenuItem("Tarotro/Configs/Open tarot_cards Sheet", priority = 20)]
        private static void OpenTarotCards() => GoogleSheetsHelper.OpenSheetByName("Configs/tarot_cards");

        [MenuItem("Tarotro/Configs/Open enemies Sheet", priority = 21)]
        private static void OpenEnemies() => GoogleSheetsHelper.OpenSheetByName("Configs/enemies");

        [MenuItem("Tarotro/Configs/Open battle Sheet", priority = 22)]
        private static void OpenBattle() => GoogleSheetsHelper.OpenSheetByName("Configs/battle");
    }

    public static class GoogleSheetsHelper {
        private const string ConfigsAssetPath =
            "Assets/Settings/GoogleSheetConfigs/Google Sheets Configs.asset";

        public static void PullAll() {
            var configs = LoadConfigs();
            if (configs == null) return;

            configs.PullAllConfigs();
            Debug.Log("[GoogleSheetsMenu] All configs pulled successfully.");
        }

        public static void OpenSpreadsheet() {
            var configs = LoadConfigs();
            if (configs == null) return;

            foreach (var config in configs.Configs) {
                configs.OpenSpreadsheet(config);
            }
        }

        public static void OpenSheetByName(string configName) {
            var configs = LoadConfigs();
            if (configs == null) return;

            foreach (var group in configs.Configs) {
                foreach (var sheet in group.Sheets) {
                    if (sheet.ConfigName != configName) continue;

                    Application.OpenURL(
                        $"https://docs.google.com/spreadsheets/d/{group.SpreadSheet}/#gid={sheet.SheetId}");
                    return;
                }
            }

            Debug.LogWarning($"[GoogleSheetsMenu] Sheet '{configName}' not found.");
        }

        private static GoogleSheetsConfigs LoadConfigs() {
            var asset = AssetDatabase.LoadAssetAtPath<GoogleSheetsConfigs>(ConfigsAssetPath);
            if (asset != null) return asset;

            Debug.LogError(
                $"[GoogleSheetsMenu] GoogleSheetsConfigs asset not found at '{ConfigsAssetPath}'.");
            return null;
        }
    }
}
