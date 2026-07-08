using System;
class Program {
    static void Main() {
        try {
            var dt = DateTime.Parse("2026-05-01");
            Console.WriteLine(dt.ToString("dd/MM/yyyy"));
        } catch (Exception ex) {
            Console.WriteLine("Parse Error: " + ex.Message);
        }
    }
}
