using System;
using System.IO;
using System.Data;
using System.Reflection;

class Program
{
    static void Main()
    {
        try
        {
            Assembly asm = Assembly.LoadFrom(@"SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");
            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                conn.Open();
                
                string sqlPath = @"App_Data\sp_BC_KetQuaHoatDongKinhDoanh_GetList.sql";
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
                Console.WriteLine("SP sp_BC_KetQuaHoatDongKinhDoanh_GetList Executed successfully in Database.");
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
