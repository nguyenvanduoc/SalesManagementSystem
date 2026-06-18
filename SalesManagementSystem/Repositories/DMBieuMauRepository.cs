using System;
using System.Collections.Generic;
using System.Linq;
using Dapper;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models.Entities;
using SalesManagementSystem.Models.ViewModels;
using SalesManagementSystem.Repositories.Interfaces;

namespace SalesManagementSystem.Repositories
{
    public class DMBieuMauRepository : IDMBieuMauRepository
    {
        private readonly DbConnectionFactory _db;

        public DMBieuMauRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<DMBieuMauViewModel> GetPaged(int page, int pageSize, string keyword, out int totalRecords)
        {
            using (var conn = _db.CreateConnection())
            {
                var searchPattern = string.IsNullOrEmpty(keyword) ? null : $"%{keyword}%";
                
                var sqlCount = @"
                    SELECT COUNT(*) 
                    FROM DM_BieuMau 
                    WHERE @Keyword IS NULL OR MaBieuMau LIKE @Keyword OR TenBieuMau LIKE @Keyword";
                
                totalRecords = conn.ExecuteScalar<int>(sqlCount, new { Keyword = searchPattern });

                var sqlList = @"
                    SELECT 
                        bm.ID, bm.MaBieuMau, bm.TenBieuMau, bm.TenFile, bm.DuoiFile, bm.NgayTao, bm.NguoiTao,
                        ISNULL(nv.HoDem, '') + ' ' + ISNULL(nv.Ten, '') as TenNguoiTao
                    FROM DM_BieuMau bm
                    LEFT JOIN NS_NhanSu nv ON bm.NguoiTao = nv.ID
                    WHERE @Keyword IS NULL OR bm.MaBieuMau LIKE @Keyword OR bm.TenBieuMau LIKE @Keyword
                    ORDER BY bm.NgayTao DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                var parameters = new
                {
                    Keyword = searchPattern,
                    Offset = (page - 1) * pageSize,
                    PageSize = pageSize
                };

                return conn.Query<DMBieuMauViewModel>(sqlList, parameters).ToList();
            }
        }

        public DM_BieuMau GetById(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<DM_BieuMau>("SELECT * FROM DM_BieuMau WHERE ID = @ID", new { ID = id });
            }
        }

        public DM_BieuMau GetByMa(string maBieuMau)
        {
            using (var conn = _db.CreateConnection())
            {
                return conn.QueryFirstOrDefault<DM_BieuMau>("SELECT * FROM DM_BieuMau WHERE MaBieuMau = @MaBieuMau", new { MaBieuMau = maBieuMau });
            }
        }

        public bool CheckDuplicateCode(string maBieuMau, int currentId = 0)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = "SELECT COUNT(*) FROM DM_BieuMau WHERE MaBieuMau = @MaBieuMau AND ID <> @ID";
                var count = conn.ExecuteScalar<int>(sql, new { MaBieuMau = maBieuMau, ID = currentId });
                return count > 0;
            }
        }

        public int Insert(DM_BieuMau bieuMau)
        {
            using (var conn = _db.CreateConnection())
            {
                var sql = @"
                    INSERT INTO DM_BieuMau (MaBieuMau, TenBieuMau, TenFile, DuoiFile, NoiDung, NgayTao, NguoiTao)
                    VALUES (@MaBieuMau, @TenBieuMau, @TenFile, @DuoiFile, @NoiDung, @NgayTao, @NguoiTao);
                    SELECT CAST(SCOPE_IDENTITY() as int);";
                return conn.QuerySingle<int>(sql, bieuMau);
            }
        }

        public void Update(DM_BieuMau bieuMau)
        {
            using (var conn = _db.CreateConnection())
            {
                // Note: Only update file if a new one is provided.
                var sql = @"
                    UPDATE DM_BieuMau 
                    SET MaBieuMau = @MaBieuMau, 
                        TenBieuMau = @TenBieuMau
                        " + (bieuMau.NoiDung != null ? ", TenFile = @TenFile, DuoiFile = @DuoiFile, NoiDung = @NoiDung" : "") + @"
                    WHERE ID = @ID";
                conn.Execute(sql, bieuMau);
            }
        }

        public void Delete(int id)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Execute("DELETE FROM DM_BieuMau WHERE ID = @ID", new { ID = id });
            }
        }
    }
}
