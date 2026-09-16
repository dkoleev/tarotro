using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Yogi.UniGSC.Editor.Parsers;

namespace Tarotro.Editor.ConfigParsers {
    [ParserType("grouped")]
    public class GroupedParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            var headers = sheetData[0];
            var result = new JObject();
            var groups = BuildGroups(sheetData);

            foreach (var group in groups) {
                var id = group[0][0].ToString();
                var item = new JObject();

                for (var col = 1; col < headers.Count; col++) {
                    var rawHeader = headers[col].ToString();
                    if (string.IsNullOrEmpty(rawHeader))
                        continue;

                    var forceArray = rawHeader.EndsWith("[]");
                    var header = forceArray ? rawHeader.Substring(0, rawHeader.Length - 2) : rawHeader;
                    var values = CollectColumnValues(group, col);

                    if (forceArray || values.Count > 1)
                        item[header] = new JArray(values.ToArray());
                    else if (values.Count == 1)
                        item[header] = JToken.FromObject(values[0]);
                }

                result[id] = item;
            }

            return result.ToString();
        }

        private static List<List<IList<object>>> BuildGroups(IList<IList<object>> sheetData) {
            var groups = new List<List<IList<object>>>();
            List<IList<object>> current = null;

            for (var i = 1; i < sheetData.Count; i++) {
                var row = sheetData[i];
                var hasId = row.Count > 0 && !string.IsNullOrEmpty(row[0]?.ToString());

                if (hasId) {
                    current = new List<IList<object>> { row };
                    groups.Add(current);
                } else {
                    current?.Add(row);
                }
            }

            return groups;
        }

        private static List<object> CollectColumnValues(List<IList<object>> group, int col) {
            var values = new List<object>();

            foreach (var row in group) {
                if (col >= row.Count)
                    continue;

                var cell = row[col]?.ToString();
                if (string.IsNullOrEmpty(cell))
                    continue;

                if (cell.Contains(", ")) {
                    var parts = cell.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var part in parts) {
                        values.Add(SpreadSheetsParserUtils.GetParseValue(part.Trim()));
                    }
                } else {
                    values.Add(SpreadSheetsParserUtils.GetParseValue(row[col]));
                }
            }

            return values;
        }
    }
}
