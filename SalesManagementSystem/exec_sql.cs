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
                conn.Open();
                Console.WriteLine("Connection opened successfully.");
                
                string sqlPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\create_dashboard_stored_procedure.sql";
                string sql = File.ReadAllText(sqlPath);
                string[] batches = sql.Split(new[] { "\r\nGO", "\nGO", "GO\r", "GO\n" }, StringSplitOptions.RemoveEmptyEntries);
                
                foreach(string batch in batches)
                {
                    if(string.IsNullOrWhiteSpace(batch)) continue;
                    using (IDbCommand cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = batch;
                        cmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("SP Executed successfully.");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
