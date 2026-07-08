using System;
using System.IO;
using System.Text;

class Program
{
    static void Main()
    {
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
    }
}
