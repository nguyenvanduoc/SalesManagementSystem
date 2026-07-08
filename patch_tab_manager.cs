using System;
using System.IO;

class Program
{
    static void Main()
    {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Scripts\tab-manager.js";
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            
            if (!content.Contains("window.showLoadingLocal = showLoadingLocal;"))
            {
                string target = "return {";
                string replacement = "window.showLoadingLocal = showLoadingLocal;\n    window.hideLoadingLocal = hideLoadingLocal;\n\n    return {";
                content = content.Replace(target, replacement);
                File.WriteAllText(path, content);
                Console.WriteLine("Patched tab-manager.js");
            }
            else
            {
                Console.WriteLine("Already patched");
            }
        }
    }
}
