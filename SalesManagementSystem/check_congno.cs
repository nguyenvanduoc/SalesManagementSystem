using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string webConfigPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Web.config";
            var map = new ExeConfigurationFileMap { ExeConfigFilename = webConfigPath };
            var config = ConfigurationManager.OpenMappedExeConfiguration(map, ConfigurationUserLevel.None);
            
            string connStr = null;
            foreach(ConnectionStringSettings cs in config.ConnectionStrings.ConnectionStrings) {
                if (cs.Name == "LocalSqlServer") continue;
                connStr = cs.ConnectionString;
                break;
            }
            if (string.IsNullOrEmpty(connStr)) {
                string datPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat";
                if (System.IO.File.Exists(datPath)) {
                    var bytes = System.IO.File.ReadAllBytes(datPath);
                    string b64 = System.Text.Encoding.UTF8.GetString(bytes);
                    var b64b = Convert.FromBase64String(b64);
                    string raw = System.Text.Encoding.UTF8.GetString(b64b);
                    var rawParts = raw.Split(new[] { "@@" }, StringSplitOptions.None);
                    if (rawParts.Length >= 2) {
                        connStr = rawParts[1];
                    }
                }
            }
            if (string.IsNullOrEmpty(connStr)) {
                connStr = "Server=localhost;Database=SalesManagementSystem;Trusted_Connection=True;"; 
            }

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                using (var cmd = new SqlCommand("EXEC sp_helptext 'sp_CongNo_PhaseTra_NCC_GetList'", conn)) {
                    using (var reader = cmd.ExecuteReader()) {
                        while(reader.Read()) {
                            Console.Write(reader[0]);
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
