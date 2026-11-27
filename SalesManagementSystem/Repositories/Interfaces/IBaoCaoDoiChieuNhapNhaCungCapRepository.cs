using System;
using System.Collections.Generic;
using SalesManagementSystem.Models.ViewModels;

namespace SalesManagementSystem.Repositories.Interfaces
{
    public interface IBaoCaoDoiChieuNhapNhaCungCapRepository
    {
        IEnumerable<BaoCaoDoiChieuNhapNhaCungCapViewModel> GetList(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay, string soChungTu = null);
        IEnumerable<dynamic> GetNhaCungCapDropdown();

        /// <summary>
        /// Tổng hợp công nợ phải trả theo từng nhà cung cấp: số dư đầu kỳ (trước tuNgay),
        /// phát sinh Nợ (nhập) / Có (thanh toán) trong kỳ. Chỉ trả NCC có số liệu khác 0.
        /// </summary>
        IEnumerable<BaoCaoTongHopCongNoPhaiTraViewModel> GetTongHopCongNoPhaiTra(int? idNhaCungCap, DateTime tuNgay, DateTime denNgay);
    }
}
