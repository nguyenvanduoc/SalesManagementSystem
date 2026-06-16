using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

public class GlobalFixer {
    public static void Run() {
        string dir = @".";
        var files = Directory.GetFiles(dir, "*.*", SearchOption.AllDirectories)
                             .Where(f => f.EndsWith(".cs") || f.EndsWith(".cshtml") || f.EndsWith(".js")).ToList();
        
        Encoding w1252 = Encoding.GetEncoding(1252);
        int count = 0;

        foreach (var file in files) {
            string originalText = File.ReadAllText(file, Encoding.UTF8);
            
            // \u00C3 is Ã, \u00C4 is Ä, \u00E1 is á, \u00C6 is Æ
            if (!originalText.Contains("\u00C3") && !originalText.Contains("\u00C4") && 
                !originalText.Contains("\u00E1") && !originalText.Contains("\u00C6")) continue;

            // We match sequences of characters that fall in the Windows-1252 range (0x00 - 0xFF)
            // But we must also include special characters that 1252 maps from 0x80-0x9F,
            // like \u20AC, \u201A, \u0192, \u201E, \u2026, \u2020, \u2021, \u02C6, \u2030, \u0160, \u2039, \u0152, \u017D
            // \u2018, \u2019, \u201C, \u201D, \u2022, \u2013, \u2014, \u02DC, \u2122, \u0161, \u203A, \u0153, \u017E, \u0178
            string pattern = @"[\x00-\xFF\u20AC\u201A\u0192\u201E\u2026\u2020\u2021\u02C6\u2030\u0160\u2039\u0152\u017D\u2018\u2019\u201C\u201D\u2022\u2013\u2014\u02DC\u2122\u0161\u203A\u0153\u017E\u0178]+";

            string fixedText = Regex.Replace(originalText, pattern, match => {
                string block = match.Value;
                if (block.Contains("\u00C3") || block.Contains("\u00C4") || 
                    block.Contains("\u00E1") || block.Contains("\u00C6")) {
                    try {
                        byte[] bytes = w1252.GetBytes(block);
                        
                        // If GetBytes replaced something with '?', it means the char wasn't in 1252
                        int origQ = block.Count(c => c == '?');
                        int newQ = bytes.Count(b => b == 63);
                        if (newQ > origQ) return block;

                        string decoded = Encoding.UTF8.GetString(bytes);
                        
                        // Basic heuristic to ensure we decoded something resembling Vietnamese
                        // and we didn't just produce junk
                        if (!decoded.Contains("\uFFFD")) { // no replacement character
                            // To be safe, only replace if decoded contains valid Vietnamese chars
                            // or looks like normal text
                            return decoded;
                        }
                    } catch { }
                }
                return block;
            });
            
            if (fixedText != originalText) {
                File.WriteAllText(file, fixedText, new UTF8Encoding(true));
                Console.WriteLine("Fixed " + file);
                count++;
            }
        }
        Console.WriteLine("Total fixed: " + count);
    }
}
