using System.Collections.Generic;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class NhatKyChungRepository : INhatKyChungRepository
    {
        private readonly DbConnectionFactory _db;

        public NhatKyChungRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<NhatKyChungListViewModel> GetList(string tuNgay, string denNgay, string soChungTu, string taiKhoanNo, string taiKhoanCo, string loaiChungTu)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@TuNgay", string.IsNullOrEmpty(tuNgay) ? null : tuNgay);
                p.Add("@DenNgay", string.IsNullOrEmpty(denNgay) ? null : denNgay);
                p.Add("@SoChungTu", string.IsNullOrEmpty(soChungTu) ? null : soChungTu);
                p.Add("@TaiKhoanNo", string.IsNullOrEmpty(taiKhoanNo) ? null : taiKhoanNo);
                p.Add("@TaiKhoanCo", string.IsNullOrEmpty(taiKhoanCo) ? null : taiKhoanCo);
                p.Add("@LoaiChungTu", string.IsNullOrEmpty(loaiChungTu) ? null : loaiChungTu);

                return conn.Query<NhatKyChungListViewModel>("sp_KT_NhatKyChung_GetList", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void Insert(KT_NhatKyChung entity)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@NgayChungTu", entity.NgayChungTu);
                p.Add("@SoChungTu", entity.SoChungTu);
                p.Add("@LoaiChungTu", entity.LoaiChungTu);
                p.Add("@IDChungTu", entity.IDChungTu);
                p.Add("@TaiKhoanNo", entity.TaiKhoanNo);
                p.Add("@TaiKhoanCo", entity.TaiKhoanCo);
                p.Add("@SoTien", entity.SoTien);
                p.Add("@DienGiai", entity.DienGiai);
                p.Add("@NguoiTao", entity.NguoiTao);

                conn.Execute("sp_KT_NhatKyChung_Insert", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }

        public void Cancel(string loaiChungTu, int idChungTu, int nguoiHuy)
        {
            using (var conn = _db.CreateConnection())
            {
                var p = new DynamicParameters();
                p.Add("@LoaiChungTu", loaiChungTu);
                p.Add("@IDChungTu", idChungTu);
                p.Add("@NguoiHuy", nguoiHuy);

                conn.Execute("sp_KT_NhatKyChung_Cancel", p, commandType: System.Data.CommandType.StoredProcedure);
            }
        }
    }
}
