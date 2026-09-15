using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Yogi.UniGSC.Editor.Parsers;

namespace Tarotro.Editor.ConfigParsers {
    [ParserType("battle")]
    public class BattleParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            var result = new JObject();

            for (var i = 0; i < sheetData.Count; i++) {
                var key = sheetData[i][0].ToString();
                var value = SpreadSheetsParserUtils.GetParseValue(sheetData[i][1]);

                SetNestedValue(result, key, value);
            }

            return result.ToString();
        }

        private static void SetNestedValue(JObject root, string key, object value) {
            var parts = key.Split('.');

            var current = root;
            for (var i = 0; i < parts.Length - 1; i++) {
                if (current[parts[i]] is not JObject child) {
                    child = new JObject();
                    current[parts[i]] = child;
                }

                current = child;
            }

            current[parts[^1]] = JToken.FromObject(value);
        }
    }
}
