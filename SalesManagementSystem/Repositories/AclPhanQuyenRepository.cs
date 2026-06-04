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
    public class AclPhanQuyenRepository : IAclPhanQuyenRepository
    {
        private readonly DbConnectionFactory _db;

        public AclPhanQuyenRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public IEnumerable<PhanQuyenTreeViewModel> GetTreeLogin()
        {
            var sql = @"
                SELECT 
                    l.ID, 
                    l.IDThamChieu, 
                    ISNULL(nv.MaNhanVien, '') + ' - ' + ISNULL(l.HoDem, '') + ' ' + ISNULL(l.Ten, '') as TenNhanVien,
                    l.TenDangNhap
                FROM ACL_Login l
                LEFT JOIN NS_NhanVien nv ON l.IDNhanVien = nv.ID
                WHERE l.IsActive = 1
            ";

            using (var conn = _db.CreateConnection())
            {
                var flatList = conn.Query<PhanQuyenTreeViewModel>(sql).ToList();
                return BuildTree(flatList, null);
            }
        }

        private List<PhanQuyenTreeViewModel> BuildTree(List<PhanQuyenTreeViewModel> flatList, int? parentId)
        {
            var nodes = flatList.Where(x => x.IDThamChieu == parentId).ToList();
            foreach (var node in nodes)
            {
                node.Children = BuildTree(flatList, node.ID);
            }
            return nodes;
        }

        public IEnumerable<PhanQuyenMatrixViewModel> GetMatrixQuyen(int idLogin)
        {
            using (var conn = _db.CreateConnection())
            {
                // 1. Get all screens
                var screens = conn.Query<AclManHinh>("SELECT * FROM ACL_ManHinh WHERE IsSuDung = 1 ORDER BY STT").ToList();

                // 2. Get all actions
                var actions = conn.Query<AclAction>("SELECT * FROM ACL_Action").ToList();

                // 3. Get permissions for this user
                var pqSql = "SELECT IDAction FROM ACL_PhanQuyen WHERE IDLogin = @IDLogin AND IsChoPhep = 1";
                var grantedActions = conn.Query<int>(pqSql, new { IDLogin = idLogin }).ToHashSet();

                var matrix = new List<PhanQuyenMatrixViewModel>();

                var groups = screens.GroupBy(s => s.NhomChaManHinh).ToList();

                foreach (var group in groups)
                {
                    var matrixGroup = new PhanQuyenMatrixViewModel
                    {
                        NhomChaManHinh = string.IsNullOrEmpty(group.Key) ? "KHÁC" : group.Key
                    };

                    foreach (var screen in group)
                    {
                        var screenVM = new PhanQuyenScreenVM
                        {
                            IDManHinh = screen.ID,
                            TenManHinh = screen.TenManHinh
                        };

                        var screenActions = actions.Where(a => a.IDManHinh == screen.ID).ToList();
                        foreach (var action in screenActions)
                        {
                            screenVM.Actions.Add(new PhanQuyenActionVM
                            {
                                IDAction = action.ID,
                                LoaiPhanQuyen = action.LoaiPhanQuyen,
                                GhiChu = action.GhiChu,
                                IsChoPhep = grantedActions.Contains(action.ID)
                            });
                        }

                        matrixGroup.Screens.Add(screenVM);
                    }

                    matrix.Add(matrixGroup);
                }

                return matrix;
            }
        }

        public bool SaveQuyen(int idLogin, List<int> checkedActionIds, int currentUser)
        {
            using (var conn = _db.CreateConnection())
            {
                conn.Open();
                using (var trans = conn.BeginTransaction())
                {
                    try
                    {
                        if (checkedActionIds == null)
                        {
                            checkedActionIds = new List<int>();
                        }

                        // Lấy các IDAction đã có trong DB
                        var existingIds = conn.Query<int>("SELECT IDAction FROM ACL_PhanQuyen WHERE IDLogin = @IDLogin", new { IDLogin = idLogin }, trans).ToList();

                        var toInsert = checkedActionIds.Except(existingIds).ToList();
                        var toUpdate1 = checkedActionIds.Intersect(existingIds).ToList();
                        var toUpdate0 = existingIds.Except(checkedActionIds).ToList();

                        if (toInsert.Any())
                        {
                            var sqlInsert = @"
                                INSERT INTO ACL_PhanQuyen (IDLogin, IDAction, IsChoPhep, NgayTao, NguoiTao) 
                                VALUES (@IDLogin, @IDAction, 1, @NgayTao, @NguoiTao)";
                            var insertData = toInsert.Select(idAction => new
                            {
                                IDLogin = idLogin,
                                IDAction = idAction,
                                NgayTao = DateTime.Now,
                                NguoiTao = currentUser
                            }).ToList();
                            conn.Execute(sqlInsert, insertData, trans);
                        }

                        if (toUpdate1.Any())
                        {
                            var sqlUpdate1 = @"
                                UPDATE ACL_PhanQuyen 
                                SET IsChoPhep = 1, NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat 
                                WHERE IDLogin = @IDLogin AND IDAction = @IDAction";
                            var update1Data = toUpdate1.Select(idAction => new
                            {
                                IDLogin = idLogin,
                                IDAction = idAction,
                                NgayCapNhat = DateTime.Now,
                                NguoiCapNhat = currentUser
                            }).ToList();
                            conn.Execute(sqlUpdate1, update1Data, trans);
                        }

                        if (toUpdate0.Any())
                        {
                            var sqlUpdate0 = @"
                                UPDATE ACL_PhanQuyen 
                                SET IsChoPhep = 0, NgayCapNhat = @NgayCapNhat, NguoiCapNhat = @NguoiCapNhat 
                                WHERE IDLogin = @IDLogin AND IDAction = @IDAction";
                            var update0Data = toUpdate0.Select(idAction => new
                            {
                                IDLogin = idLogin,
                                IDAction = idAction,
                                NgayCapNhat = DateTime.Now,
                                NguoiCapNhat = currentUser
                            }).ToList();
                            conn.Execute(sqlUpdate0, update0Data, trans);
                        }

                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        return false;
                    }
                }
            }
        }

        public List<int> GetParentActionIds(int idLogin)
        {
            using (var conn = _db.CreateConnection())
            {
                var parentId = conn.QueryFirstOrDefault<int?>("SELECT IDThamChieu FROM ACL_Login WHERE ID = @ID", new { ID = idLogin });
                if (parentId == null)
                {
                    return null;
                }
                return conn.Query<int>("SELECT IDAction FROM ACL_PhanQuyen WHERE IDLogin = @ParentID AND IsChoPhep = 1", new { ParentID = parentId }).ToList();
            }
        }
    }
}
