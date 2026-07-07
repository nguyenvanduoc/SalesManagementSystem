using System;
using System.Reflection;

class Program {
    static void Main() {
        try {
            string webConfigPath = "c:\\Users\\duoc0\\OneDrive\\Desktop\\WEB_QLBH\\QuanLyBanHang\\SalesManagementSystem\\SalesManagementSystem\\bin\\SalesManagementSystem.dll";
            var assembly = Assembly.LoadFrom(webConfigPath);
            var type = assembly.GetType("SalesManagementSystem.Helpers.Security.ConfigManager");
            var method = type.GetMethod("GetConnectionString", BindingFlags.Public | BindingFlags.Static);
            string connStr = (string)method.Invoke(null, new object[] { "DefaultConnection" });
            Console.WriteLine(connStr);
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
