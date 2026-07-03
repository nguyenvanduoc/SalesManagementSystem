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

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                using (var cmd = new SqlCommand("sp_KT_PhieuChi_GetList", conn)) {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader()) {
                        bool hasCol = false;
                        for (int i = 0; i < reader.FieldCount; i++) {
                            if (reader.GetName(i).Equals("SoTienPhanBo", StringComparison.OrdinalIgnoreCase)) {
                                hasCol = true;
                                break;
                            }
                        }
                        Console.WriteLine("Has SoTienPhanBo column: " + hasCol);
                        
                        if (!hasCol) return;
                        
                        // Check data
                        int count = 0;
                        while(reader.Read() && count < 5) {
                            Console.WriteLine("ID: " + reader["ID"] + ", SoTienChi: " + reader["SoTienChi"] + ", SoTienPhanBo: " + reader["SoTienPhanBo"]);
                            count++;
                        }
                    }
                }
            }
        } catch (Exception ex) {
            Console.WriteLine(ex.Message);
        }
    }
}
