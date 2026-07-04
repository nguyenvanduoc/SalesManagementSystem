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
                    cmd.CommandText = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'KHO_PhieuNhap' AND COLUMN_NAME = 'ConLai'";
                    conn.Open();
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Console.Write(reader.GetString(0));
                        }
                    }
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine("ERROR: " + ex.ToString());
        }
    }
}
