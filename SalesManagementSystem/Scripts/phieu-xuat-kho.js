var PhieuXuatKho = (function () {
    var table;
    var tableModal;

    var initDataTable = function () {
        if ($('#tablePhieuXuatKho').length === 0) return;

        table = $('#tablePhieuXuatKho').DataTable({
            "processing": true,
            "serverSide": false,
            "ajax": {
                "url": "/PhieuXuatKho/GetList",
                "type": "GET",
                "data": function (d) {
                    d.tuNgay = $('#tuNgay').val();
                    d.denNgay = $('#denNgay').val();
                    d.soChungTu = $('#soChungTu').val();
                    d.trangThai = $('#trangThai').val();
                }
            },
            "columns": [
                { "data": "SoChungTu" },
                { "data": "NgayXuat", "render": function(data) { return data ? data.substring(0, 10) : ''; } },
                { "data": "SoDonHang" },
                { "data": "TenKhachHang" },
                { "data": "TenKhoHang" },
                { "data": "GhiChu" },
                { 
                    "data": "TrangThaiDonHang",
                    "render": function(data) {
                        if (data === 2) return '<span class="badge badge-info">Đã duyệt</span>';
                        if (data === 3) return '<span class="badge badge-success">Đã xuất kho</span>';
                        return '';
                    }
                },
                { 
                    "data": "TrangThai",
                    "render": function(data) {
                        if (data === 1) return '<span class="badge badge-warning">Đề nghị ghi</span>';
                        if (data === 2) return '<span class="badge badge-success">Đã ghi sổ</span>';
                        if (data === 3) return '<span class="badge badge-danger">Đã hủy</span>';
                        return '';
                    }
                },
                {
                    "data": "ID",
                    "render": function(data, type, row) {
                        var html = '<div class="btn-group">';
                        
                        if (row.TrangThai === 1) {
                            html += '<button type="button" class="btn btn-sm btn-success" onclick="PhieuXuatKho.ghiSo(' + data + ')" title="Ghi sổ"><i class="fas fa-check"></i> Ghi sổ</button>';
                            html += '<button type="button" class="btn btn-sm btn-danger" onclick="PhieuXuatKho.huy(' + data + ')" title="Hủy"><i class="fas fa-times"></i> Hủy</button>';
                        }

                        html += '</div>';
                        return html;
                    }
                }
            ],
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/Vietnamese.json"
            }
        });
    };

    var loadData = function () {
        if (table) {
            table.ajax.reload();
        }
    };

    var openDonDatHangModal = function () {
        $('#modalContainer').load('/PhieuXuatKho/GetModalChonDon', function() {
            $('#modalChonDonDatHang').modal('show');
            initDonDatHangTable();
        });
    };

    var initDonDatHangTable = function() {
        if (tableModal) {
            tableModal.destroy();
        }
        tableModal = $('#tableDonDatHangList').DataTable({
            "processing": true,
            "serverSide": false,
            "ajax": {
                "url": "/PhieuXuatKho/GetDonDatHangDaDuyet",
                "type": "GET",
                "dataSrc": "data.Items"
            },
            "columns": [
                { "data": "SoDonHang" },
                { "data": "NgayDonHang", "render": function(data) { return data ? data.substring(0, 10) : ''; } },
                { "data": "TenKhachHang" },
                { "data": "TenKhoHang" },
                { "data": "TongCong", "render": $.fn.dataTable.render.number(',', '.', 0, '') },
                { "data": "TrangThaiDon", "render": function() { return '<span class="badge badge-info">Đã duyệt</span>'; } },
                {
                    "data": "ID",
                    "render": function(data) {
                        return '<a href="/PhieuXuatKho/Create?idDonDatHang=' + data + '" class="btn btn-sm btn-primary">Chọn</a>';
                    }
                }
            ],
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/Vietnamese.json"
            }
        });
    };

    var save = function () {
        var data = $('#frmSavePX').serialize();
        
        $.ajax({
            url: '/PhieuXuatKho/Save',
            type: 'POST',
            data: data,
            success: function (res) {
                if (res.success) {
                    alert("Lưu thành công!");
                    window.location.href = '/PhieuXuatKho/Index';
                } else {
                    alert("Lỗi: " + res.message);
                }
            },
            error: function () {
                alert("Đã xảy ra lỗi kết nối.");
            }
        });
    };

    var ghiSo = function (id) {
        if (confirm("Bạn có chắc muốn ghi sổ phiếu xuất này? Hệ thống sẽ trừ tồn kho và ghi nhận Giá Vốn Hàng Bán (Nợ 632 / Có 156).")) {
            $.ajax({
                url: '/PhieuXuatKho/GhiSo',
                type: 'POST',
                data: { id: id },
                success: function (res) {
                    if (res.success) {
                        alert("Ghi sổ thành công!");
                        loadData();
                    } else {
                        alert("Lỗi: " + res.message);
                    }
                }
            });
        }
    };

    var huy = function (id) {
        var lyDo = prompt("Nhập lý do hủy:");
        if (lyDo != null) {
            $.ajax({
                url: '/PhieuXuatKho/Huy',
                type: 'POST',
                data: { id: id, lyDo: lyDo },
                success: function (res) {
                    if (res.success) {
                        alert("Hủy thành công!");
                        loadData();
                    } else {
                        alert("Lỗi: " + res.message);
                    }
                }
            });
        }
    };

    return {
        init: function () {
            initDataTable();
        },
        loadData: loadData,
        openDonDatHangModal: openDonDatHangModal,
        save: save,
        ghiSo: ghiSo,
        huy: huy
    };
})();
