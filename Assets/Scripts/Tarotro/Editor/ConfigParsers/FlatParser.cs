using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Yogi.UniGSC.Editor.Parsers;

namespace Tarotro.Editor.ConfigParsers {
    [ParserType("flat")]
    public class FlatParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            var headers = sheetData[0];
            var result = new JObject();

            for (var i = 1; i < sheetData.Count; i++) {
                var row = sheetData[i];
                var item = new JObject();

                for (var j = 1; j < row.Count; j++) {
                    if (j >= headers.Count)
                        break;

                    var rawHeader = headers[j].ToString();
                    if (string.IsNullOrEmpty(rawHeader))
                        continue;

                    var forceArray = rawHeader.EndsWith("[]");
                    var header = forceArray ? rawHeader.Substring(0, rawHeader.Length - 2) : rawHeader;
                    var cell = row[j]?.ToString();

                    if (string.IsNullOrEmpty(cell))
                        continue;

                    if (forceArray || cell.Contains(", ")) {
                        var parts = cell.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
                        var arr = new JArray();
                        foreach (var part in parts) {
                            arr.Add(JToken.FromObject(SpreadSheetsParserUtils.GetParseValue(part.Trim())));
                        }
                        item[header] = arr;
                    } else {
                        item[header] = JToken.FromObject(SpreadSheetsParserUtils.GetParseValue(row[j]));
                    }
                }

                result[row[0].ToString()] = item;
            }

            return result.ToString();
        }
    }
}
