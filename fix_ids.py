import re

path = r'c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views\Dashboard\Index.cshtml'

with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

# Add uid variable at the top
if 'string uid = Guid.NewGuid().ToString(' not in content:
    content = content.replace('ViewBag.Title = "Dashboard";', 'ViewBag.Title = "Dashboard";\n    string uid = Guid.NewGuid().ToString("N");')

# Define a list of IDs to make unique
ids = ['dbTuNgay', 'dbDenNgay', 'btnLoadDashboard', 'btnResetDashboard', 'valDonHang', 'valDoanhThu', 'trendDoanhThu', 'valSpTon', 'trendTongSLTon', 'trendSpSapHet', 'valLoiNhuan', 'trendLoiNhuan', 'valCongNoKH', 'valCongNoNCC', 'valTienMat', 'valTongSoDu', 'tbRecentOrders', 'chartDoanhThu', 'valTongDonHang', 'chartDonHang', 'valGiaTriTonKho', 'valSpSapHet', 'chartBanChay', 'chartTonKho', 'valTongThu', 'valTongChi', 'valDongTien', 'chartThuChi', 'listTaiKhoan', 'wDonHang', 'wPhieuNhap', 'wTaiKhoan', 'wTonKho', 'wChungTu', 'listHoatDong', 'cnkhTongNo', 'cnkhSoDoiTuong', 'cnkhNoLonNhat', 'cnkhTenNoLonNhat', 'tbCongNoKH', 'cnnccTongNo', 'cnnccSoDoiTuong', 'cnnccNoLonNhat', 'cnnccTenNoLonNhat', 'tbCongNoNCC', 'tbTopKH', 'tbTopNCC', 'currentDateText']

for i in ids:
    # Replace id="name" with id="name_@uid"
    content = re.sub(rf'id=\"{i}\"', f'id=\"{i}_@uid\"', content)
    # Replace $('#name') with $('#name_@uid')
    content = re.sub(rf'\$\(\'#{i}\'\)', f"$('#{i}_@uid')", content)
    # Replace document.getElementById('name')
    content = re.sub(rf'document\.getElementById\(\'{i}\'\)', f"document.getElementById('{i}_@uid')", content)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

print('Done!')
