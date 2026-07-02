using System;
using System.Data.SqlClient;

class Program {
    static void Main() {
        try {
            string connString = @"Data Source=.;Initial Catalog=SalesManagementSystem;Integrated Security=True";
            using (var conn = new SqlConnection(connString)) {
                conn.Open();
                using (var cmd = conn.CreateCommand()) {
                    cmd.CommandText = "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE COLUMN_NAME IN ('DonGiaVon', 'ThanhTienVon');";
                    using (var reader = cmd.ExecuteReader()) {
                        Console.WriteLine("--- SCHEMA CHECK ---");
                        while(reader.Read()) {
                            Console.WriteLine(string.Format("{0} - {1} - {2}", reader[0], reader[1], reader[2]));
                        }
                    }
                    
                    cmd.CommandText = @"
                        SELECT TOP 10 IDSanPham, SoLuongNhap, SoLuongXuat, DonGia, ThanhTien, DonGiaVon, ThanhTienVon 
                        FROM KHO_GiaoDichKho 
                        WHERE IDSanPham IN (
                            SELECT ID FROM DM_SanPham WHERE MaSanPham IN ('SP04', 'SP05', 'SP02')
                        ) ORDER BY IDSanPham, NgayChungTu;";
                    using (var reader = cmd.ExecuteReader()) {
                        Console.WriteLine("--- KHO_GiaoDichKho DATA CHECK ---");
                        while(reader.Read()) {
                            Console.WriteLine(string.Format("{0} | Nhập:{1} Xuất:{2} Giá:{3} TT:{4} GiáVốn:{5} TTVốn:{6}", reader[0], reader[1], reader[2], reader[3], reader[4], reader[5], reader[6]));
                        }
                    }

                    cmd.CommandText = @"
                        SELECT TOP 10 IDSanPham, SoLuong, DonGia, ThanhTien, DonGiaVon, ThanhTienVon
                        FROM BAN_ChungTuBanHang_ChiTiet
                        WHERE IDSanPham IN (
                            SELECT ID FROM DM_SanPham WHERE MaSanPham IN ('SP04', 'SP05', 'SP02')
                        );";
                    using (var reader = cmd.ExecuteReader()) {
                        Console.WriteLine("--- BAN_ChungTuBanHang_ChiTiet DATA CHECK ---");
                        while(reader.Read()) {
                            Console.WriteLine(string.Format("{0} | Xuất:{1} Giá:{2} TT:{3} GiáVốn:{4} TTVốn:{5}", reader[0], reader[1], reader[2], reader[3], reader[4], reader[5]));
                        }
                    }
                }
            }
        } catch(Exception ex) {
            Console.WriteLine(ex.ToString());
        }
    }
}
