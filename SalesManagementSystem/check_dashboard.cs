using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;

class Program
{
    static void Main()
    {
        try
        {
            Assembly asm = Assembly.LoadFrom(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");
                var repoType = asm.GetType("SalesManagementSystem.Repositories.DashboardRepository");
                var repo = Activator.CreateInstance(repoType, factory);
                var method = repoType.GetMethod("GetDashboardData");
                var data = method.Invoke(repo, new object[] { new DateTime(2026, 7, 1), new DateTime(2026, 7, 31, 23, 59, 59) });
                
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(data);
                Console.WriteLine("JSON Output:");
                Console.WriteLine(json.Substring(0, Math.Min(json.Length, 500)));
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
