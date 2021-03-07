using System;
using System.Collections.Generic;

namespace FezBotRedux.Common.Extensions {
    public static class StringExtensions {

        public static List<string> WordWrap(this string input, int maxCharacters) {
            var lines = new List<string>();

            if (!input.Contains(" ") && !input.Contains("\n")) {
                var start = 0;
                while (start < input.Length) {
                    lines.Add(input.Substring(start, Math.Min(maxCharacters, input.Length - start)));
                    start += maxCharacters;
                }
            } else {
                var paragraphs = input.Split('\n');

                foreach (var paragraph in paragraphs) {
                    var words = paragraph.Split(' ');

                    var line = "";
                    foreach (var word in words) {
                        if ((line + word).Length > maxCharacters) {
                            lines.Add(line.Trim());
                            line = "";
                        }

                        line += string.Format("{0} ", word);
                    }

                    if (line.Length > 0) {
                        lines.Add(line.Trim());
                    }
                }
            }
            return lines;
        }
    }
}

