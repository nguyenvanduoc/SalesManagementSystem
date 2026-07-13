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
            Assembly asm = Assembly.LoadFrom(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\bin\SalesManagementSystem.dll");
            Type factoryType = asm.GetType("SalesManagementSystem.Data.DbConnectionFactory");
            object factory = Activator.CreateInstance(factoryType);
            
            MethodInfo createConnMethod = factoryType.GetMethod("CreateConnection");
            using (IDbConnection conn = (IDbConnection)createConnMethod.Invoke(factory, null))
            {
                conn.Open();
                
                using (IDbCommand cmd = conn.CreateCommand())
                {
                    cmd.CommandText = "sp_helptext 'sp_KT_PhieuChi_GetList'";
                    using (IDataReader reader = cmd.ExecuteReader())
                    {
                        using (StreamWriter writer = new StreamWriter(@"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\sp_out2.txt"))
                        {
                            while(reader.Read())
                            {
                                writer.Write(reader.GetString(0));
                            }
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
