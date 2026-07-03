using System;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        try {
            string webConfigPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config";
            var map = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            string connStr = config.ConnectionStrings.ConnectionStrings["DefaultConnection"]?.ConnectionString;
            
            // Wait, we don't know the connection string name. Let's list all!
            foreach(ConnectionStringSettings cs in config.ConnectionStrings.ConnectionStrings) {
                if (cs.Name == "LocalSqlServer") continue;
                connStr = cs.ConnectionString;
                break;
            }

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                using (var cmd = new SqlCommand("sp_helptext", conn)) {
                    cmd.CommandType = System.Data.CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@objname", "sp_KT_PhieuChi_GetList");
                    using (var reader = cmd.ExecuteReader()) {
                        var sb = new StringBuilder();
                        while(reader.Read()) {
                            sb.Append(reader.GetString(0));
                        }
                        File.WriteAllText("c:\\Users\\duoc0\\OneDrive\\Desktop\\WEB_QLBH\\QuanLyBanHang\\SalesManagementSystem\\SalesManagementSystem\\sp_def.txt", sb.ToString());
                    }
                }
                Console.WriteLine("Done.");
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
