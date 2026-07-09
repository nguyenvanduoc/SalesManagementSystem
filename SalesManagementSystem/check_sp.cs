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
                    cmd.CommandText = "SELECT COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Acl_ManHinh' ORDER BY ORDINAL_POSITION";
                    conn.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        var sb = new System.Text.StringBuilder();
                        while (reader.Read())
                        {
                            sb.AppendLine(string.Format("{0} - {1}", reader["COLUMN_NAME"], reader["DATA_TYPE"]));
                        }
                        File.WriteAllText(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Data\tables.txt", sb.ToString());
                    }
                    Console.WriteLine("Done");
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
