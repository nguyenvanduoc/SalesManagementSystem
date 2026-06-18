using System;
using System.Text;
using System.IO;

class Program {
    static void Main() {
        string text = "KhÃ´ng tÃ¬m tháº¥y Ä‘Æ¡n hÃ ng.";
        Encoding w1252 = Encoding.GetEncoding(1252);
        byte[] bytes = w1252.GetBytes(text);
        Console.WriteLine(Encoding.UTF8.GetString(bytes));
    }
}
 