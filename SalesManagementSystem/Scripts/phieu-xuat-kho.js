var PhieuXuatKho = (function () {
    var config = {
        chiTietsJson: [],
        selectedKhoId: null,
        selectedKhoText: '',
        selectedNhanSuId: null,
        selectedNhanSuText: '',
        searchKhoUrl: '',
        searchNhanSuUrl: '',
        searchSpUrl: ''
    };

    function _formatNumber(n) {
        if (!n) return '0';
        var parts = n.toString().split(".");
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".");
        return parts.join(",");
    }

    function _parseMoney(str) {
        if (!str) return 0;
        str = str.toString().replace(/\./g, '').replace(/,/g, '.');
        var val = parseFloat(str);
        return isNaN(val) ? 0 : val;
    }

    function init(options) {
        $.extend(config, options);
        _initSelect2();
        _initEvents();
        _renderTable(config.chiTietsJson);
    }

    function _initSelect2() {
        $('#IDKho').select2({
            placeholder: '-- Chọn kho --',
            ajax: {
                url: config.searchKhoUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        $('#IDNhanSuNhan').select2({
            placeholder: '-- Chọn người nhận --',
            allowClear: true,
            ajax: {
                url: config.searchNhanSuUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        $('#IDNhanSuNhan').on('change', function () {
            var selectedData = $(this).select2('data');
            if (selectedData && selectedData.length > 0) {
                var data = selectedData[0];
                if (data.id) {
                    if (data.hoten) $('#TenNguoiNhan').val(data.hoten);
                    if (data.sdt) $('#SoDienThoaiNguoiNhan').val(data.sdt);
                } else {
                    $('#TenNguoiNhan').val('');
                    $('#SoDienThoaiNguoiNhan').val('');
                }
            }
        });

        if (config.selectedKhoId) {
            $('#IDKho').append(new Option(config.selectedKhoText, config.selectedKhoId, true, true)).trigger('change');
        }
        if (config.selectedNhanSuId) {
            $('#IDNhanSuNhan').append(new Option(config.selectedNhanSuText, config.selectedNhanSuId, true, true)).trigger('change');
        }
    }

    function _initSanPhamSelect2($row, ctData) {
        var $select = $row.find('.sel-sanpham');
        $select.select2({
            placeholder: 'Gõ để tìm SP...',
            ajax: {
                url: config.searchSpUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        if (ctData && ctData.IDSanPham) {
            var text = ctData.MaSanPham + ' - ' + ctData.TenSanPham;
            $select.append(new Option(text, ctData.IDSanPham, true, true)).trigger('change');
        }

        $select.on('select2:select', function (e) {
            var data = e.params.data;
            $row.find('.txt-dvt').val(data.dvt || '');
            if (!_parseMoney($row.find('.txt-soluong').val())) {
                $row.find('.txt-soluong').val(1);
            }
            calcRow($row);
        });
    }

    function _renderTable(dataList) {
        var $tbody = $('#tblChiTiet tbody');
        $tbody.empty();

        if (dataList && dataList.length > 0) {
            $.each(dataList, function (i, ct) { _addRowWithData($tbody, ct); });
        } else {
            _addRowWithData($tbody, null);
        }
        _updateTotal();
    }

    function _addRowWithData($tbody, ct) {
        var id = ct ? ct.ID : 0;
        var soLuong = ct ? ct.SoLuong : 1;
        var donGia = ct ? ct.DonGia : 0;
        var thue = ct ? ct.ThueGTGT : 0;
        var ghiChu = ct ? ct.GhiChu || '' : '';
        var dvt = ct ? ct.DVT || '' : '';

        var html = '<tr data-id="' + id + '">' +
            '  <td class="text-center align-middle row-stt"></td>' +
            '  <td><select class="form-control sel-sanpham" style="width:100%"></select></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-dvt" readonly value="' + dvt + '" /></td>' +
            '  <td><input type="text" class="form-control txt-soluong text-end" inputmode="numeric" value="' + _formatNumber(soLuong) + '" /></td>' +
            '  <td><input type="text" class="form-control txt-dongia text-end" inputmode="decimal" value="' + _formatNumber(donGia) + '" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-thanhtien text-end" readonly value="0" /></td>' +
            '  <td><input type="number" class="form-control txt-thue text-end" step="0.1" value="' + thue + '" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-tienthue text-end" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-tongsauthue text-end" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control txt-ghichu" value="' + ghiChu + '" /></td>' +
            '  <td class="text-center align-middle">' +
            '    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-row"><i class="bi bi-trash"></i></button>' +
            '  </td>' +
            '</tr>';

        var $row = $(html);
        $tbody.append($row);

        _initSanPhamSelect2($row, ct);

        $row.find('.txt-soluong').on('input change', function () {
            var val = $(this).val();
            var sanitized = val.replace(/[^0-9.]/g, '');
            if (val !== sanitized) $(this).val(sanitized);
            calcRow($row);
        });

        $row.find('.txt-dongia, .txt-thue').on('input change', function () {
            calcRow($row);
        });

        $row.find('.txt-dongia, .txt-soluong').on('blur change', function () {
            $(this).val(_formatNumber(_parseMoney($(this).val())));
        });

        calcRow($row);
        _updateSTT();
    }

    function _initEvents() {
        $('#btnAddRow').on('click', function () {
            _addRowWithData($('#tblChiTiet tbody'), null);
        });

        $('#tblChiTiet').on('click', '.btn-remove-row', function () {
            $(this).closest('tr').remove();
            _updateSTT();
            _updateTotal();
        });
    }

    function _updateSTT() {
        $('#tblChiTiet tbody tr').each(function (idx) {
            $(this).find('.row-stt').text(idx + 1);
        });
    }

    function calcRow($row) {
        var sl = _parseMoney($row.find('.txt-soluong').val());
        var dg = _parseMoney($row.find('.txt-dongia').val());
        var thuePt = parseFloat($row.find('.txt-thue').val()) || 0;

        var thanhTien = sl * dg;
        var tienThue = thanhTien * thuePt / 100;
        var tong = thanhTien + tienThue;

        $row.find('.txt-thanhtien').val(_formatNumber(thanhTien));
        $row.find('.txt-tienthue').val(_formatNumber(tienThue));
        $row.find('.txt-tongsauthue').val(_formatNumber(tong));

        _updateTotal();
    }

    function _updateTotal() {
        var totalTienHang = 0;
        var totalTienThue = 0;
        var totalCong = 0;

        $('#tblChiTiet tbody tr').each(function () {
            totalTienHang += _parseMoney($(this).find('.txt-thanhtien').val());
            totalTienThue += _parseMoney($(this).find('.txt-tienthue').val());
            totalCong += _parseMoney($(this).find('.txt-tongsauthue').val());
        });

        $('#dispTongTienHang').text(_formatNumber(totalTienHang));
        $('#dispTongTienThue').text(_formatNumber(totalTienThue));
        $('#dispTongCong').text(_formatNumber(totalCong));
        $('#dispTongCongMini').text(_formatNumber(totalCong));
    }

    function validateAndSerialize() {
        var isValid = true;
        var errorMsg = '';

        if (!$('#NgayXuat').val()) { errorMsg += 'Ngày xuất không được để trống.\n'; isValid = false; }
        if (!$('#IDKho').val()) { errorMsg += 'Vui lòng chọn Kho.\n'; isValid = false; }

        // Trong phiếu xuất kho, có thể chọn IDNhanSuNhan hoặc nhập TenNguoiNhan
        var idNhanSu = $('#IDNhanSuNhan').val();
        var tenNguoiNhan = $('#TenNguoiNhan').val();
        if (!idNhanSu && !tenNguoiNhan) {
            errorMsg += 'Vui lòng chọn Nhân sự nhận hoặc nhập Tên người nhận ngoài.\n';
            isValid = false;
        }

        var chiTiets = [];
        var rows = $('#tblChiTiet tbody tr');
        if (rows.length === 0) {
            errorMsg += 'Phiếu xuất phải có ít nhất 1 mặt hàng.\n';
            isValid = false;
        } else {
            rows.each(function (idx) {
                var stt = idx + 1;
                var $row = $(this);
                var spId = $row.find('.sel-sanpham').val();
                if (!spId) { errorMsg += 'Dòng ' + stt + ': Chưa chọn sản phẩm.\n'; isValid = false; }

                var sl = _parseMoney($row.find('.txt-soluong').val());
                if (sl <= 0) { errorMsg += 'Dòng ' + stt + ': Số lượng phải > 0.\n'; isValid = false; }

                var dg = _parseMoney($row.find('.txt-dongia').val());
                if (dg < 0) { errorMsg += 'Dòng ' + stt + ': Đơn giá không được âm.\n'; isValid = false; }

                var thue = parseFloat($row.find('.txt-thue').val()) || 0;
                if (thue < 0) { errorMsg += 'Dòng ' + stt + ': Thuế GTGT không được âm.\n'; isValid = false; }

                if (isValid) {
                    chiTiets.push({
                        ID: parseInt($row.attr('data-id')) || 0,
                        IDSanPham: parseInt(spId),
                        STT: stt,
                        SoLuong: sl,
                        DonGia: dg,
                        ThanhTien: _parseMoney($row.find('.txt-thanhtien').val()),
                        ThueGTGT: thue,
                        TienThue: _parseMoney($row.find('.txt-tienthue').val()),
                        TongSauThue: _parseMoney($row.find('.txt-tongsauthue').val()),
                        GhiChu: $row.find('.txt-ghichu').val()
                    });
                }
            });
        }

        if (!isValid) {
            if (typeof showToast === 'function') {
                showToast('warning', errorMsg.replace(/\n/g, '<br/>'));
            } else {
                alert(errorMsg);
            }
            return false;
        }

        return {
            ID: $('#ID').val(),
            SoChungTu: $('#SoChungTu').val(),
            NgayXuat: $('#NgayXuat').val(),
            IDKho: $('#IDKho').val(),
            IDNhanSuNhan: $('#IDNhanSuNhan').val(),
            TenNguoiNhan: $('#TenNguoiNhan').val(),
            SoDienThoaiNguoiNhan: $('#SoDienThoaiNguoiNhan').val(),
            GhiChu: $('#GhiChu').val(),
            ChiTiets: chiTiets
        };
    }

    return {
        init: init,
        validateAndSerialize: validateAndSerialize
    };
})();
