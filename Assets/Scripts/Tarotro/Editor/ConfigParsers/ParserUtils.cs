using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Yogi.UniGSC.Editor.Parsers;

namespace Tarotro.Editor.ConfigParsers {
    public static class ParserUtils {
        public static (string name, bool isArray) ParseHeader(string rawHeader) {
            if (rawHeader.EndsWith("[]"))
                return (rawHeader.Substring(0, rawHeader.Length - 2), true);
            return (rawHeader, false);
        }

        public static List<object> ParseCell(string cell) {
            var values = new List<object>();
            if (cell.Contains(", ")) {
                foreach (var part in cell.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries))
                    values.Add(SpreadSheetsParserUtils.GetParseValue(part.Trim()));
            } else {
                values.Add(SpreadSheetsParserUtils.GetParseValue(cell));
            }
            return values;
        }

        public static JToken ToJsonToken(List<object> values, bool forceArray) {
            if (values.Count == 0)
                return null;
            if (forceArray || values.Count > 1)
                return new JArray(values.ToArray());
            return JToken.FromObject(values[0]);
        }
    }
}
