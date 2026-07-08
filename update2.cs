using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;

class Program
{
    static void Main()
    {
        // 2. Update Controllers
        string controllersPath = @"SalesManagementSystem\Controllers";
        foreach (var file in Directory.GetFiles(controllersPath, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file, Encoding.UTF8);
            string original = content;

            content = Regex.Replace(content, @"filterContext\.ActionParameters\[""tuNgay""\] = new DateTime\(DateTime\.Now\.Year,\s*DateTime\.Now\.Month,\s*1\)\.ToString\(""yyyy-MM-dd""\);", "filterContext.ActionParameters[\"tuNgay\"] = new DateTime(DateTime.Now.Year, 1, 1).ToString(\"yyyy-MM-dd\");");
            content = Regex.Replace(content, @"filterContext\.ActionParameters\[""denNgay""\] = new DateTime\(DateTime\.Now\.Year,\s*DateTime\.Now\.Month,\s*DateTime\.DaysInMonth\(DateTime\.Now\.Year,\s*DateTime\.Now\.Month\)\)\.ToString\(""yyyy-MM-dd""\);", "filterContext.ActionParameters[\"denNgay\"] = DateTime.Now.ToString(\"yyyy-MM-dd\");");
            
            content = Regex.Replace(content, @"DateTime\.Now\.Month,\s*1", "1, 1");
            content = Regex.Replace(content, @"now\.Month,\s*1", "1, 1");
            
            content = Regex.Replace(content, @"new DateTime\(now\.Year,\s*now\.Month,\s*DateTime\.DaysInMonth\(now\.Year,\s*now\.Month\)\)", "DateTime.Now");
            content = Regex.Replace(content, @"new DateTime\(DateTime\.Now\.Year,\s*DateTime\.Now\.Month,\s*DateTime\.DaysInMonth\(DateTime\.Now\.Year,\s*DateTime\.Now\.Month\)\)", "DateTime.Now");

            if (content != original)
            {
                File.WriteAllText(file, content, new UTF8Encoding(true));
                Console.WriteLine("Updated " + Path.GetFileName(file));
            }
        }

        // 3. Update Views (HTML and JS)
        string viewsPath = @"SalesManagementSystem\Views";
        foreach (var file in Directory.GetFiles(viewsPath, "*.cshtml", SearchOption.AllDirectories))
        {
            if (file.EndsWith("_Layout.cshtml")) continue;

            string content = File.ReadAllText(file, Encoding.UTF8);
            string original = content;

            if (content.Contains("<option value=\"current_month\">") && !content.Contains("<option value=\"current_year\">"))
            {
                content = content.Replace("<option value=\"current_month\">Đầu tháng đến hiện tại</option>", 
                    "<option value=\"current_year\">Từ đầu năm đến hiện tại</option>\r\n                                <option value=\"current_month\">Đầu tháng đến hiện tại</option>");
            }

            if (content.Contains(".val('current_month')"))
            {
                content = content.Replace(".val('current_month')", ".val('current_year')");
            }

            if (content != original)
            {
                File.WriteAllText(file, content, new UTF8Encoding(true));
                Console.WriteLine("Updated " + Path.GetFileName(file));
            }
        }
    }
}
