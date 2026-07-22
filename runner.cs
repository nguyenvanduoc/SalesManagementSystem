using System;
using System.Reflection;
using System.Configuration;

class Program {
    static void Main() {
        try {
            AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", @"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config");
            
            var a = Assembly.LoadFrom(@"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            var t = a.GetType("SalesManagementSystem.Helpers.Security.ConfigManager");
            var m = t.GetMethod("GetConnectionString");
            var res = m.Invoke(null, new object[] { "DefaultConnection" });
            Console.WriteLine(res);
        } catch (Exception ex) { Console.WriteLine(ex); }
    }
}
