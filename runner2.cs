using System;
using System.Reflection;
using System.Configuration;

class Program {
    static void Main() {
        try {
            var a = Assembly.LoadFrom(@"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            var t = a.GetType("SalesManagementSystem.Helpers.Security.ConfigManager");
            var m = t.GetMethod("GetConnectionString", BindingFlags.Public | BindingFlags.Static);
            
            var prop = typeof(ConfigurationManager).GetProperty("AppSettings");
            var settings = (System.Collections.Specialized.NameValueCollection)prop.GetValue(null, null);
            var fi = typeof(System.Collections.Specialized.NameObjectCollectionBase).GetField("_readOnly", BindingFlags.Instance | BindingFlags.NonPublic);
            fi.SetValue(settings, false);
            
            settings["ConfigFile"] = @"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\App_Config\system.dat";
            settings["KeyPart1"] = "VanDuoc@123123!";
            
            var res = m.Invoke(null, new object[] { "DefaultConnection" });
            Console.WriteLine(res);
        } catch (Exception ex) { Console.WriteLine(ex); }
    }
}
