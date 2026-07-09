using System;
using System.IO;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

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
            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    string sql = File.ReadAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\sp_Dashboard_GetData_ALTER.sql");
                    cmd.CommandText = sql;
                    conn.Open();
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Altered SP successfully!");
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
