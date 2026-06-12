var ChungTuBanHang = (function () {
    var table;

    var initDataTable = function () {
        if ($('#tableChungTuBanHang').length === 0) return;

        table = $('#tableChungTuBanHang').DataTable({
            "processing": true,
            "serverSide": false,
            "ajax": {
                "url": "/ChungTuBanHang/GetList",
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
                { "data": "NgayChungTu", "render": function(data) { return data ? data.substring(0, 10) : ''; } },
                { "data": "SoDonHang" },
                { "data": "TenKhachHang" },
                { "data": "TenKhoHang" },
                { "data": "TongCong", "render": $.fn.dataTable.render.number(',', '.', 0, '') },
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
                            html += '<button type="button" class="btn btn-sm btn-success" onclick="ChungTuBanHang.ghiSo(' + data + ')" title="Ghi sổ"><i class="fas fa-check"></i> Ghi sổ</button>';
                            html += '<button type="button" class="btn btn-sm btn-danger" onclick="ChungTuBanHang.huy(' + data + ')" title="Hủy"><i class="fas fa-times"></i> Hủy</button>';
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

    var save = function (isGhiSo) {
        var actionText = isGhiSo ? "lưu và ghi" : "lưu dữ liệu";
        var confirmHtml = '<div class="modal fade" id="confirmSaveModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">' +
            '<div class="modal-dialog modal-dialog-centered" style="max-width: 420px;">' +
                '<div class="modal-content shadow-lg" style="background-color: #ffffff; color: #333; border: none; border-top: 5px solid #ffc107; border-radius: 6px;">' +
                    '<div class="modal-body p-4">' +
                        '<div class="d-flex align-items-center mb-3">' +
                            '<i class="bi bi-exclamation-triangle-fill text-warning me-3" style="font-size: 1.6rem;"></i>' +
                            '<h5 class="mb-0" style="font-size: 1.3rem; font-weight: 600; color: #333;">Xác nhận!</h5>' +
                        '</div>' +
                        '<p class="mb-4" style="font-size: 1rem; color: #555;">Bạn có muốn ' + actionText + ' chứng từ này không?</p>' +
                        '<div class="d-flex justify-content-end gap-2">' +
                            '<button type="button" class="btn" id="btnConfirmSaveAction" style="background-color: #ffc107; color: #fff; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem;">ĐỒNG Ý</button>' +
                            '<button type="button" class="btn" data-bs-dismiss="modal" style="background-color: #e9ecef; color: #333; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem; border: none;">ĐỂ SAU</button>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        if ($('#confirmSaveModal').length === 0) {
            $('body').append(confirmHtml);
        } else {
            $('#confirmSaveModal').replaceWith(confirmHtml);
        }

        $('#btnConfirmSaveAction').on('click', function() {
            var btn = $(this);
            btn.prop('disabled', true);
            var data = $('#frmSaveCTBH').serialize();
            data += '&ghiSo=' + (isGhiSo ? 'true' : 'false');
                
                $.ajax({
                    url: '/ChungTuBanHang/Save',
                    type: 'POST',
                    data: data,
                    success: function (res) {
                        btn.prop('disabled', false);
                        var modalInstance = bootstrap.Modal.getInstance(document.getElementById('confirmSaveModal'));
                        if (modalInstance) modalInstance.hide();
                        
                        if (res.success) {
                            if (typeof showToast !== 'undefined') showToast('success', 'Lưu thành công!');
                            // Không redirect hoặc đóng form theo yêu cầu
                        } else {
                            if (typeof showToast !== 'undefined') showToast('error', 'Lỗi: ' + res.message);
                        }
                    },
                    error: function () {
                        btn.prop('disabled', false);
                        var modalInstance = bootstrap.Modal.getInstance(document.getElementById('confirmSaveModal'));
                        if (modalInstance) modalInstance.hide();
                        
                        if (typeof showToast !== 'undefined') showToast('error', 'Đã xảy ra lỗi kết nối.');
                    }
                });
        });

        var myModal = new bootstrap.Modal(document.getElementById('confirmSaveModal'));
        myModal.show();
    };

    var ghiSo = function (id) {
        var confirmHtml = '<div class="modal fade" id="confirmGhiSoModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">' +
            '<div class="modal-dialog modal-dialog-centered" style="max-width: 420px;">' +
                '<div class="modal-content shadow-lg" style="background-color: #ffffff; color: #333; border: none; border-top: 5px solid #28a745; border-radius: 6px;">' +
                    '<div class="modal-body p-4">' +
                        '<div class="d-flex align-items-center mb-3">' +
                            '<i class="bi bi-info-circle-fill text-success me-3" style="font-size: 1.6rem;"></i>' +
                            '<h5 class="mb-0" style="font-size: 1.3rem; font-weight: 600; color: #333;">Ghi dữ liệu chứng từ!</h5>' +
                        '</div>' +
                        '<p class="mb-4" style="font-size: 1rem; color: #555;">Hành động này sẽ tự động sinh phiếu xuất kho và bút toán kế toán Doanh Thu. Không thể hoàn tác.</p>' +
                        '<div class="d-flex justify-content-end gap-2">' +
                            '<button type="button" class="btn" id="btnConfirmGhiSoAction" style="background-color: #28a745; color: #fff; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem;">ĐỒNG Ý</button>' +
                            '<button type="button" class="btn" data-bs-dismiss="modal" style="background-color: #e9ecef; color: #333; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem; border: none;">ĐỂ SAU</button>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        if ($('#confirmGhiSoModal').length === 0) {
            $('body').append(confirmHtml);
        } else {
            $('#confirmGhiSoModal').replaceWith(confirmHtml);
        }

        $('#btnConfirmGhiSoAction').on('click', function() {
            var btn = $(this);
            btn.prop('disabled', true);
            $.ajax({
                    url: '/ChungTuBanHang/GhiSo',
                    type: 'POST',
                    data: { id: id },
                    success: function (res) {
                        btn.prop('disabled', false);
                        var modalInstance = bootstrap.Modal.getInstance(document.getElementById('confirmGhiSoModal'));
                        if (modalInstance) modalInstance.hide();
                        
                        if (res.success) {
                            if (typeof showToast !== 'undefined') showToast('success', 'Ghi dữ liệu thành công!');
                            loadData();
                        } else {
                            if (typeof showToast !== 'undefined') showToast('error', 'Lỗi: ' + res.message);
                        }
                    },
                    error: function() {
                        btn.prop('disabled', false);
                        var modalInstance = bootstrap.Modal.getInstance(document.getElementById('confirmGhiSoModal'));
                        if (modalInstance) modalInstance.hide();
                        
                        if (typeof showToast !== 'undefined') showToast('error', 'Đã xảy ra lỗi kết nối.');
                    }
                });
        });

        var myModal = new bootstrap.Modal(document.getElementById('confirmGhiSoModal'));
        myModal.show();
    };

    var huy = function (id) {
        var confirmHtml = '<div class="modal fade" id="confirmHuyModal" tabindex="-1" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">' +
            '<div class="modal-dialog modal-dialog-centered" style="max-width: 420px;">' +
                '<div class="modal-content shadow-lg" style="background-color: #ffffff; color: #333; border: none; border-top: 5px solid #dc3545; border-radius: 6px;">' +
                    '<div class="modal-body p-4">' +
                        '<div class="d-flex align-items-center mb-3">' +
                            '<i class="bi bi-x-circle-fill text-danger me-3" style="font-size: 1.6rem;"></i>' +
                            '<h5 class="mb-0" style="font-size: 1.3rem; font-weight: 600; color: #333;">Hủy chứng từ!</h5>' +
                        '</div>' +
                        '<div class="mb-3">' +
                            '<label class="form-label text-muted fw-bold">Nhập lý do hủy:</label>' +
                            '<input type="text" class="form-control" id="txtLyDoHuy" placeholder="Lý do hủy..." />' +
                        '</div>' +
                        '<div class="d-flex justify-content-end gap-2">' +
                            '<button type="button" class="btn" id="btnConfirmHuyAction" style="background-color: #dc3545; color: #fff; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem;">HỦY CTBH</button>' +
                            '<button type="button" class="btn" data-bs-dismiss="modal" style="background-color: #e9ecef; color: #333; font-weight: bold; border-radius: 4px; padding: 8px 16px; font-size: 0.9rem; border: none;">ĐỂ SAU</button>' +
                        '</div>' +
                    '</div>' +
                '</div>' +
            '</div>' +
        '</div>';

        if ($('#confirmHuyModal').length === 0) {
            $('body').append(confirmHtml);
        } else {
            $('#confirmHuyModal').replaceWith(confirmHtml);
        }

        $('#btnConfirmHuyAction').on('click', function() {
            var lyDo = $('#txtLyDoHuy').val().trim();
            if (!lyDo) {
                if (typeof showToast !== 'undefined') showToast('warning', 'Vui lòng nhập lý do hủy!');
                return;
            }

            var btn = $(this);
            btn.prop('disabled', true);
            $.ajax({
                    url: '/ChungTuBanHang/Huy',
                    type: 'POST',
                    data: { id: id, lyDo: lyDo },
                    success: function (res) {
                        btn.prop('disabled', false);
                        if (res.success) {
                            var modalInstance = bootstrap.Modal.getInstance(document.getElementById('confirmHuyModal'));
                            if (modalInstance) modalInstance.hide();
                            if (typeof showToast !== 'undefined') showToast('success', 'Hủy thành công!');
                            loadData();
                        } else {
                            if (typeof showToast !== 'undefined') showToast('error', 'Lỗi: ' + res.message);
                        }
                    },
                    error: function() {
                        btn.prop('disabled', false);
                        if (typeof showToast !== 'undefined') showToast('error', 'Đã xảy ra lỗi kết nối.');
                    }
                });
        });

        var myModal = new bootstrap.Modal(document.getElementById('confirmHuyModal'));
        myModal.show();
    };

    var tableModal;
    var openDonDatHangModal = function () {
        $('#modalContainer').load('/ChungTuBanHang/GetModalChonDon', function() {
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
                "url": "/ChungTuBanHang/GetDonDatHangChuaLap",
                "type": "GET",
                "dataSrc": "data"
            },
            "columns": [
                { "data": "SoDonHang" },
                { "data": "NgayTaoDon", "render": function(data) { return data ? data.substring(0, 10) : ''; } },
                { "data": "TenKhachHang" },
                { "data": "ThanhTienHang", "render": $.fn.dataTable.render.number(',', '.', 0, '') },
                { "data": "ThanhTienThue", "render": $.fn.dataTable.render.number(',', '.', 0, '') },
                { "data": "TongTien", "render": $.fn.dataTable.render.number(',', '.', 0, '') },
                { "data": "TenTrangThai" },
                { 
                    "data": "ConPhaiLapCTBH",
                    "render": function(data) {
                        if (data === 'Còn') {
                            return '<span class="badge bg-warning text-dark">Còn</span>';
                        }
                        return '<span class="badge bg-secondary">Hết</span>';
                    }
                },
                {
                    "data": "ID",
                    "render": function(data, type, row) {
                        if (row.ConPhaiLapCTBH === 'Hết') {
                            return '<button class="btn btn-sm btn-secondary text-white" disabled>Chọn</button>';
                        }
                        return '<a href="/ChungTuBanHang/Create?idDonDatHang=' + data + '" class="btn btn-sm btn-primary text-white">Chọn</a>';
                    }
                }
            ],
            "language": {
                "url": "//cdn.datatables.net/plug-ins/1.10.21/i18n/Vietnamese.json"
            }
        });
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
