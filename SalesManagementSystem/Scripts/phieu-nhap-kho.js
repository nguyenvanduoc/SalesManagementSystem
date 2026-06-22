var PhieuNhapKho = (function () {
    var config = {
        chiTietsJson: [],
        selectedKhoId: null,
        selectedKhoText: '',
        selectedNccId: null,
        selectedNccText: '',
        selectedKhoId: null,
        selectedKhoText: '',
        selectedNccId: null,
        selectedNccText: '',
        selectedNccText: '',
        selectedPhuongTienId: null,
        selectedPhuongTienText: '',
        searchKhoUrl: '',
        searchNccUrl: '',
        searchSpUrl: '',
        searchPhuongTienUrl: ''
    };

    function _formatNumber(n) {
        if (n === '' || n == null) return '';
        if (n === 0 || n === '0') return '0';
        if (!n) return '0';
        var parts = n.toString().split(".");
        parts[0] = parts[0].replace(/\B(?=(\d{3})+(?!\d))/g, ".");
        return parts.join(",");
    }

    function _parseMoney(str) {
        if (str === '' || str == null) return 0;
        if (!str && str !== 0) return 0;
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
            minimumInputLength: 0,
            ajax: {
                url: config.searchKhoUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        $('#IDNhaCungCap').select2({
            placeholder: '-- Chọn nhà cung cấp --',
            minimumInputLength: 0,
            ajax: {
                url: config.searchNccUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        $('#IDPhuongTien').select2({
            placeholder: '-- Chọn phương tiện --',
            allowClear: true,
            minimumInputLength: 0,
            ajax: {
                url: config.searchPhuongTienUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        if (config.selectedKhoId) {
            $('#IDKho').append(new Option(config.selectedKhoText, config.selectedKhoId, true, true)).trigger('change');
        }
        if (config.selectedNccId) {
            $('#IDNhaCungCap').append(new Option(config.selectedNccText, config.selectedNccId, true, true)).trigger('change');
        }
        if (config.selectedPhuongTienId) {
            $('#IDPhuongTien').append(new Option(config.selectedPhuongTienText, config.selectedPhuongTienId, true, true)).trigger('change');
        }
    }

    function _initSanPhamSelect2($row, ctData) {
        var $select = $row.find('.sel-sanpham');
        $select.select2({
            placeholder: 'Gõ để tìm SP...',
            minimumInputLength: 0,
            ajax: {
                url: config.searchSpUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
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
            calcRow($row);
        });
    }

    function _renderTable(dataList) {
        var $tbody = $('#PNK_tblChiTiet tbody');
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
        var soLuong = ct ? ct.SoLuong : '';
        var donGia = ct ? ct.DonGia : '';
        var thue = ct ? ct.ThueGTGT : 0;
        var ghiChu = ct ? ct.GhiChu || '' : '';
        var dvt = ct ? ct.DVT || '' : '';
        var ngaySanXuat = (ct && ct.NgaySanXuat) ? ct.NgaySanXuat.split('T')[0] : '';
        var hanSuDung = (ct && ct.HanSuDung) ? ct.HanSuDung.split('T')[0] : '';

        var html = '<tr data-id="' + id + '">' +
            '  <td class="text-center align-middle row-stt"></td>' +
            '  <td><select class="form-control sel-sanpham" style="width:100%"></select></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-dvt" readonly value="' + dvt + '" /></td>' +
            '  <td><input type="text" class="form-control txt-soluong text-end" inputmode="numeric" value="' + _formatNumber(soLuong) + '" /></td>' +
            '  <td><input type="text" class="form-control txt-dongia text-end" inputmode="decimal" value="' + _formatNumber(donGia) + '" /></td>' +
            '  <td><input type="date" class="form-control txt-ngaysanxuat" value="' + ngaySanXuat + '" /></td>' +
            '  <td><input type="date" class="form-control txt-hansudung" value="' + hanSuDung + '" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-thanhtien text-end" readonly value="0" /></td>' +
            '  <td class="d-none"><input type="number" class="form-control txt-thue text-end" step="0.1" value="' + thue + '" /></td>' +
            '  <td class="d-none"><input type="text" class="form-control readonly-cell txt-tienthue text-end" readonly value="0" /></td>' +
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
        $('#PNK_btnAddRow').on('click', function () {
            _addRowWithData($('#PNK_tblChiTiet tbody'), null);
        });

        $('#PNK_tblChiTiet').on('click', '.btn-remove-row', function () {
            $(this).closest('tr').remove();
            _updateSTT();
            _updateTotal();
        });
    }

    function _updateSTT() {
        $('#PNK_tblChiTiet tbody tr').each(function (idx) {
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
        var totalSoLuong = 0;

        $('#PNK_tblChiTiet tbody tr').each(function () {
            totalSoLuong += _parseMoney($(this).find('.txt-soluong').val());
            totalTienHang += _parseMoney($(this).find('.txt-thanhtien').val());
            totalTienThue += _parseMoney($(this).find('.txt-tienthue').val());
            totalCong += _parseMoney($(this).find('.txt-tongsauthue').val());
        });

        $('#dispTongSoLuong').text(_formatNumber(totalSoLuong));
        $('#dispTongTienHang').text(_formatNumber(totalTienHang));
        $('#dispTongTienThue').text(_formatNumber(totalTienThue));
        $('#dispTongCong').text(_formatNumber(totalCong));
        $('#dispTongCongMini').text(_formatNumber(totalCong));
    }

    function validateAndSerialize() {
        var isValid = true;
        var errorMsg = '';

        if (!$('#NgayNhap').val()) { errorMsg += 'Ngày nhập không được để trống.\n'; isValid = false; }
        if (!$('#IDKho').val()) { errorMsg += 'Vui lòng chọn Kho.\n'; isValid = false; }
        if (!$('#IDNhaCungCap').val()) { errorMsg += 'Vui lòng chọn Nhà cung cấp.\n'; isValid = false; }

        var chiTiets = [];
        var rows = $('#PNK_tblChiTiet tbody tr');
        if (rows.length === 0) {
            errorMsg += 'Phiếu nhập phải có ít nhất 1 mặt hàng.\n';
            isValid = false;
        } else {
            rows.each(function (idx) {
                var stt = idx + 1;
                var $row = $(this);
                $row.removeClass('table-danger');
                var spId = $row.find('.sel-sanpham').val();
                var sl = _parseMoney($row.find('.txt-soluong').val());
                var dg = _parseMoney($row.find('.txt-dongia').val());
                var thue = _parseMoney($row.find('.txt-thue').val());

                var rowValid = true;
                if (!spId) { errorMsg += 'Dòng ' + stt + ': Chưa chọn sản phẩm.\n'; rowValid = false; }
                if (sl <= 0) { errorMsg += 'Dòng ' + stt + ': Số lượng phải > 0.\n'; rowValid = false; }
                if (dg < 0) { errorMsg += 'Dòng ' + stt + ': Đơn giá không được âm.\n'; rowValid = false; }
                if (thue < 0) { errorMsg += 'Dòng ' + stt + ': Thuế GTGT không được âm.\n'; rowValid = false; }
                
                if (!rowValid) {
                    $row.addClass('table-danger');
                    isValid = false;
                }

                if (isValid) {
                    chiTiets.push({
                        ID: parseInt($row.attr('data-id')) || 0,
                        IDSanPham: parseInt(spId),
                        SoLuong: sl,
                        DonGia: dg,
                        NgaySanXuat: $row.find('.txt-ngaysanxuat').val() || null,
                        HanSuDung: $row.find('.txt-hansudung').val() || null,
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
            NgayNhap: $('#NgayNhap').val(),
            IDKho: $('#IDKho').val(),
            IDNhaCungCap: $('#IDNhaCungCap').val(),
            SoHoaDon: $('#SoHoaDon').val(),
            NgayHoaDon: $('#NgayHoaDon').val(),
            TenNguoiGiao: $('#TenNguoiGiao').val(),
            SoDienThoaiNguoiGiao: $('#SoDienThoaiNguoiGiao').val(),
            TenNguoiNhan: $('#TenNguoiNhan').val(),
            GhiChu: $('#GhiChu').val(),
            IDPhuongTien: $('#IDPhuongTien').val(),
            NgayGiaoHang: $('#NgayGiaoHang').val(),
            HoTenTaiXe: $('#HoTenTaiXe').val(),
            SoDienThoaiTaiXe: $('#SoDienThoaiTaiXe').val(),
            ChiTiets: chiTiets
        };
    }

    return {
        init: init,
        validateAndSerialize: validateAndSerialize
    };
})();
