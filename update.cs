using System;
using System.IO;
using System.Text;
using System.Linq;

class Program
{
    static void Main()
    {
        // 1. Update Layout.cshtml
        string layoutPath = @"SalesManagementSystem\Views\Shared\_Layout.cshtml";
        if (File.Exists(layoutPath))
        {
            string content = File.ReadAllText(layoutPath, Encoding.UTF8);
            content = content.Replace("if (yTu === yDen && mTu === mDen && isFirstDay && isLastDay) {\r\n                $(this).val('month_' + mTu);\r\n            }", 
                "if (yTu === yDen && mTu === 1 && dTu === 1 && dDen === new Date().getDate() && mDen === (new Date().getMonth() + 1) && yDen === new Date().getFullYear()) {\r\n                $(this).val('current_year');\r\n            } else if (yTu === yDen && mTu === mDen && isFirstDay && isLastDay) {\r\n                $(this).val('month_' + mTu);\r\n            }");
            
            content = content.Replace("if (val === 'current_month') {", 
                "if (val === 'current_year') {\r\n                s = fDate(y, 1, 1);\r\n                e = fDate(now.getFullYear(), now.getMonth() + 1, now.getDate());\r\n            } else if (val === 'current_month') {");
            File.WriteAllText(layoutPath, content, new UTF8Encoding(true));
            Console.WriteLine("Updated _Layout.cshtml");
        }

        // 2. Update Controllers
        string controllersPath = @"SalesManagementSystem\Controllers";
        foreach (var file in Directory.GetFiles(controllersPath, "*.cs", SearchOption.AllDirectories))
        {
            string content = File.ReadAllText(file, Encoding.UTF8);
            bool changed = false;

            if (content.Contains("filterContext.ActionParameters[\"tuNgay\"] = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString(\"yyyy-MM-dd\");"))
            {
                content = content.Replace("filterContext.ActionParameters[\"tuNgay\"] = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString(\"yyyy-MM-dd\");", 
                    "filterContext.ActionParameters[\"tuNgay\"] = new DateTime(DateTime.Now.Year, 1, 1).ToString(\"yyyy-MM-dd\");");
                changed = true;
            }

            if (content.Contains("filterContext.ActionParameters[\"denNgay\"] = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString(\"yyyy-MM-dd\");"))
            {
                content = content.Replace("filterContext.ActionParameters[\"denNgay\"] = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)).ToString(\"yyyy-MM-dd\");", 
                    "filterContext.ActionParameters[\"denNgay\"] = DateTime.Now.ToString(\"yyyy-MM-dd\");");
                changed = true;
            }

            if (content.Contains("DateTime.Now.Month, 1") || content.Contains("now.Month, 1"))
            {
                content = content.Replace("DateTime.Now.Month, 1", "1, 1");
                content = content.Replace("now.Month, 1", "1, 1");
                changed = true;
            }
            
            if (content.Contains("DateTime.DaysInMonth"))
            {
                content = content.Replace("new DateTime(now.Year, now.Month, DateTime.DaysInMonth(now.Year, now.Month))", "DateTime.Now");
                content = content.Replace("new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month))", "DateTime.Now");
                changed = true;
            }

            if (changed)
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
            bool changed = false;

            if (content.Contains("<option value=\"current_month\">Đầu tháng đến hiện tại</option>") && !content.Contains("<option value=\"current_year\">Từ đầu năm đến hiện tại</option>"))
            {
                content = content.Replace("<option value=\"current_month\">Đầu tháng đến hiện tại</option>", 
                    "<option value=\"current_year\">Từ đầu năm đến hiện tại</option>\r\n                                <option value=\"current_month\">Đầu tháng đến hiện tại</option>");
                changed = true;
            }

            if (content.Contains(".val('current_month')"))
            {
                content = content.Replace(".val('current_month')", ".val('current_year')");
                changed = true;
            }

            if (changed)
            {
                File.WriteAllText(file, content, new UTF8Encoding(true));
                Console.WriteLine("Updated " + Path.GetFileName(file));
            }
        }
    }
}
