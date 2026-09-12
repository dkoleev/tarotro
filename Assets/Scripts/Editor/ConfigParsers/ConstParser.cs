using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Yogi.UniGSC.Editor.Parsers;

namespace Editor.ConfigParsers {
    [ParserType("const")]
    public class ConstParser : ISpreadsheetParser {
        public string Parse(int sheetId, IList<IList<object>> sheetData) {
            var result = new JObject();

            for (var i = 0; i < sheetData.Count; i++) {
                result.Add(new JProperty(sheetData[i][0].ToString(), sheetData[i][1].ToString()));
            }

            return result.ToString();
        }
    }
}