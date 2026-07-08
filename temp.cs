using System;
using System.Reflection;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            var assembly = Assembly.LoadFrom(@"C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            var type = assembly.GetType("SalesManagementSystem.Helpers.Security.ConfigManager");
            var method = type.GetMethod("GetConnectionString", BindingFlags.Static | BindingFlags.Public);
            if (method != null)
            {
                var connStr = method.Invoke(null, new object[] { "DefaultConnection" });
                Console.WriteLine("CONN: " + connStr);
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }
}
