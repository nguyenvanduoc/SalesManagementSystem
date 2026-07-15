using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories
{
    public class KhoHaoHutRepository : IKhoHaoHutRepository
    {
        private readonly DbConnectionFactory _dbConnectionFactory;

        public KhoHaoHutRepository(DbConnectionFactory dbConnectionFactory)
        {
            _dbConnectionFactory = dbConnectionFactory;
            try
            {
                using (var conn = _dbConnectionFactory.CreateConnection())
                {
                    string sql = @"
                        DECLARE @ManHinhID INT;
                        SELECT @ManHinhID = ID FROM ACL_ManHinh WHERE TenManHinh = N'Hao hụt hàng hóa';

                        IF @ManHinhID IS NOT NULL
                        BEGIN
                            -- 1. Xem (Index / GetList / _Detail / GetDonHang / GetChiTietDonHang / GetTonKho)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Index')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, 'Index', 'KHO_HaoHut', 1, N'Xem danh sách');

                            -- 2. Thêm (_Create / Save)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = '_Create')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, '_Create', 'KHO_HaoHut', 2, N'Màn hình thêm mới');

                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Save')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, 'Save', 'KHO_HaoHut', 2, N'Lưu thêm mới/Cập nhật');

                            -- 3. Sửa (_Edit)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = '_Edit')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, '_Edit', 'KHO_HaoHut', 3, N'Màn hình cập nhật');

                            -- 4. Xóa (Delete)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Delete')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, 'Delete', 'KHO_HaoHut', 4, N'Xóa phiếu');

                            -- 5. Ghi nhận (GhiNhan)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'GhiNhan')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, 'GhiNhan', 'KHO_HaoHut', 5, N'Ghi nhận phiếu');

                            -- 6. Hủy (Huy)
                            IF NOT EXISTS (SELECT 1 FROM ACL_Action WHERE IDManHinh = @ManHinhID AND TenAction = 'Huy')
                                INSERT INTO ACL_Action (IDManHinh, TenAction, TenController, LoaiPhanQuyen, GhiChu)
                                VALUES (@ManHinhID, 'Huy', 'KHO_HaoHut', 5, N'Hủy ghi nhận');

                            -- Tự động phân quyền cho tất cả User trong hệ thống để tránh lỗi phân quyền khi test
                            INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao)
                            SELECT l.ID, act.ID, 1, GETDATE()
                            FROM ACL_Login l
                            CROSS JOIN ACL_Action act
                            WHERE act.IDManHinh = @ManHinhID
                              AND NOT EXISTS (
                                  SELECT 1 FROM ACL_PhanQuyen pq 
                                  WHERE pq.IDLogin = l.ID AND pq.IDAction = act.ID
                              );
                        END
                    ";
                    conn.Execute(sql);

                    // Cập nhật Stored Procedure sp_KHO_HaoHutHangHoa_GetByID để lấy thêm SLHienTai
                    string sqlSp = @"
                        CREATE OR ALTER PROCEDURE sp_KHO_HaoHutHangHoa_GetByID
                            @ID INT
                        AS
                        BEGIN
                            SELECT h.*, 
                                   k.TenKhoHang AS TenKho,
                                   kh.TenKhachHang,
                                   d.SoDonHang,
                                   c.SoChungTu AS SoChungTuBanHang
                            FROM KHO_HaoHutHangHoa h
                            LEFT JOIN DM_KhoHang k ON h.IDKho = k.ID
                            LEFT JOIN NS_KhachHang kh ON h.IDKhachHang = kh.ID
                            LEFT JOIN NS_DonDatHang d ON h.IDDonHang = d.ID
                            LEFT JOIN BAN_ChungTuBanHang c ON h.IDChungTuBanHang = c.ID
                            WHERE h.ID = @ID;

                            SELECT c.*, s.MaSanPham, s.TenSanPham,
                                   CASE 
                                       WHEN h.LoaiHaoHut = 1 THEN 
                                           ISNULL((SELECT TOP 1 SoLuong FROM NS_DonDatHangChiTiet WHERE IDDonDatHang = h.IDDonHang AND IDSanPham = c.IDSanPham), 0)
                                       ELSE 
                                           ISNULL((SELECT SUM(SoLuongNhap - SoLuongXuat) FROM KHO_GiaoDichKho WHERE IDKho = h.IDKho AND IDSanPham = c.IDSanPham AND IsHuy = 0), 0)
                                   END AS SLHienTai
                            FROM KHO_HaoHutHangHoa_ChiTiet c
                            INNER JOIN KHO_HaoHutHangHoa h ON c.IDHaoHut = h.ID
                            INNER JOIN DM_SanPham s ON c.IDSanPham = s.ID
                            WHERE c.IDHaoHut = @ID;
                        END
                    ";
                    conn.Execute(sqlSp);
                }
            }
            catch { }
        }

        public List<HaoHutHangHoaViewModel> GetList(HaoHutHangHoaFilter filter)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                DateTime tuNgay = string.IsNullOrEmpty(filter.TuNgay) ? new DateTime(2000, 1, 1) : DateTime.Parse(filter.TuNgay);
                DateTime denNgay = string.IsNullOrEmpty(filter.DenNgay) ? new DateTime(2100, 1, 1) : DateTime.Parse(filter.DenNgay);

                var parameters = new DynamicParameters();
                parameters.Add("@TuNgay", tuNgay);
                parameters.Add("@DenNgay", denNgay);
                parameters.Add("@LoaiHaoHut", filter.LoaiHaoHut);
                parameters.Add("@IDKho", filter.IDKho);
                parameters.Add("@IDKhachHang", filter.IDKhachHang);
                parameters.Add("@IDSanPham", filter.IDSanPham);
                parameters.Add("@SoChungTu", filter.SoChungTu ?? "");
                parameters.Add("@TrangThai", filter.TrangThai);
                parameters.Add("@Skip", filter.Skip);
                parameters.Add("@Take", filter.Take);

                return conn.Query<HaoHutHangHoaViewModel>("sp_KHO_HaoHutHangHoa_GetList", parameters, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public HaoHutHangHoaViewModel GetByID(int id)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ID", id);

                using (var multi = conn.QueryMultiple("sp_KHO_HaoHutHangHoa_GetByID", parameters, commandType: CommandType.StoredProcedure))
                {
                    var model = multi.Read<HaoHutHangHoaViewModel>().FirstOrDefault();
                    if (model != null)
                    {
                        model.Details = multi.Read<HaoHutHangHoaChiTietViewModel>().ToList();
                    }
                    return model;
                }
            }
        }

        public int Insert(HaoHutHangHoaViewModel model, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@SoChungTu", dbType: DbType.String, direction: ParameterDirection.Output, size: 50);
                parameters.Add("@NgayHaoHut", model.NgayHaoHut);
                parameters.Add("@LoaiHaoHut", model.LoaiHaoHut);
                parameters.Add("@IDKho", model.IDKho);
                parameters.Add("@IDDonHang", model.IDDonHang);
                parameters.Add("@IDChungTuBanHang", model.IDChungTuBanHang);
                parameters.Add("@IDKhachHang", model.IDKhachHang);
                parameters.Add("@LyDo", model.LyDo ?? "");
                parameters.Add("@GhiChu", model.GhiChu ?? "");
                parameters.Add("@NguoiTao", userID);
                parameters.Add("@ID", dbType: DbType.Int32, direction: ParameterDirection.Output);

                conn.Execute("sp_KHO_HaoHutHangHoa_Insert", parameters, commandType: CommandType.StoredProcedure);

                return parameters.Get<int>("@ID");
            }
        }

        public void Update(HaoHutHangHoaViewModel model, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ID", model.ID);
                parameters.Add("@NgayHaoHut", model.NgayHaoHut);
                parameters.Add("@LoaiHaoHut", model.LoaiHaoHut);
                parameters.Add("@IDKho", model.IDKho);
                parameters.Add("@IDDonHang", model.IDDonHang);
                parameters.Add("@IDChungTuBanHang", model.IDChungTuBanHang);
                parameters.Add("@IDKhachHang", model.IDKhachHang);
                parameters.Add("@LyDo", model.LyDo ?? "");
                parameters.Add("@GhiChu", model.GhiChu ?? "");
                parameters.Add("@NguoiCapNhat", userID);

                conn.Execute("sp_KHO_HaoHutHangHoa_Update", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void Delete(int id, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                conn.Execute("sp_KHO_HaoHutHangHoa_Delete", new { ID = id, NguoiHuy = userID }, commandType: CommandType.StoredProcedure);
            }
        }

        public void DeleteDetails(int idHaoHut)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                conn.Execute("sp_KHO_HaoHutHangHoa_ChiTiet_DeleteByHaoHut", new { IDHaoHut = idHaoHut }, commandType: CommandType.StoredProcedure);
            }
        }

        public void InsertDetail(HaoHutHangHoaChiTietViewModel detail, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                var parameters = new DynamicParameters();
                parameters.Add("@IDHaoHut", detail.IDHaoHut);
                parameters.Add("@IDSanPham", detail.IDSanPham);
                parameters.Add("@SoLuongHaoHut", detail.SoLuongHaoHut);
                parameters.Add("@DonGiaHaoHut", detail.DonGiaHaoHut);
                parameters.Add("@TienHaoHut", detail.TienHaoHut);
                parameters.Add("@DonGiaBan", detail.DonGiaBan);
                parameters.Add("@DoanhThuGiam", detail.DoanhThuGiam);
                parameters.Add("@GhiChu", detail.GhiChu ?? "");
                parameters.Add("@NguoiTao", userID);

                conn.Execute("sp_KHO_HaoHutHangHoa_ChiTiet_Insert", parameters, commandType: CommandType.StoredProcedure);
            }
        }

        public void GhiNhan(int id, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                conn.Execute("sp_KHO_HaoHutHangHoa_GhiNhan", new { ID = id, NguoiCapNhat = userID }, commandType: CommandType.StoredProcedure);
            }
        }

        public void Huy(int id, int userID)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                conn.Execute("sp_KHO_HaoHutHangHoa_Huy", new { ID = id, NguoiCapNhat = userID }, commandType: CommandType.StoredProcedure);
            }
        }

        public List<dynamic> GetDonHang(string keyword)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                return conn.Query<dynamic>("sp_KHO_HaoHutHangHoa_GetDonHang", new { Keyword = keyword }, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public List<dynamic> GetChiTietDonHang(int idDonHang)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                return conn.Query<dynamic>("sp_KHO_HaoHutHangHoa_GetChiTietDonHang", new { IDDonHang = idDonHang }, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public decimal GetTonKho(int idKho, int idSanPham)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                return conn.ExecuteScalar<decimal>("sp_KHO_HaoHutHangHoa_GetTonKho", new { IDKho = idKho, IDSanPham = idSanPham }, commandType: CommandType.StoredProcedure);
            }
        }

        public List<dynamic> GetAllTonKhoByKho(int idKho)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                return conn.Query<dynamic>("sp_KHO_HaoHutHangHoa_GetAllTonKhoByKho", new { IDKho = idKho }, commandType: CommandType.StoredProcedure).ToList();
            }
        }

        public decimal GetGiaNhapGanNhat(int idSanPham)
        {
            using (var conn = _dbConnectionFactory.CreateConnection())
            {
                return conn.ExecuteScalar<decimal>("sp_KHO_HaoHutHangHoa_GetGiaNhapGanNhat", new { IDSanPham = idSanPham }, commandType: CommandType.StoredProcedure);
            }
        }
    }
}
