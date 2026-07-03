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
                // read from app_config/system.dat
                string datPath = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\App_Config\system.dat";
                if (System.IO.File.Exists(datPath)) {
                    string content = System.IO.File.ReadAllText(datPath);
                    // It's probably encrypted, let's check DapperHelper.cs
                }
            }
            
            Console.WriteLine("ConnStr: " + connStr);
            if (string.IsNullOrEmpty(connStr)) return;

            using (var conn = new SqlConnection(connStr)) {
                conn.Open();
                
                using (var cmd = new SqlCommand("SELECT ID, SoPhieuChi, IDPhieuNhap, SoTienChi FROM KT_PhieuChi ORDER BY ID DESC", conn)) {
                    using (var reader = cmd.ExecuteReader()) {
                        Console.WriteLine("KT_PhieuChi:");
                        int count = 0;
                        while(reader.Read() && count < 10) {
                            var idpn = reader.IsDBNull(2) ? "NULL" : reader[2].ToString();
                            Console.WriteLine("ID: " + reader[0] + ", So: " + reader[1] + ", IDPhieuNhap: " + idpn + ", SoTienChi: " + reader[3]);
                            count++;
                        }
                    }
                }
                
                using (var cmd = new SqlCommand("SELECT IDPhieuChi, IDPhieuNhap, LoaiChi, SoTienPhanBo FROM KT_PhieuChiChiTiet", conn)) {
                    using (var reader = cmd.ExecuteReader()) {
                        Console.WriteLine("KT_PhieuChiChiTiet:");
                        int count = 0;
                        while(reader.Read() && count < 10) {
                            Console.WriteLine("IDPhieuChi: " + reader[0] + ", IDPhieuNhap: " + reader[1] + ", LoaiChi: " + reader[2] + ", SoTienPhanBo: " + reader[3]);
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
