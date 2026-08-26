/**
 * phieu-xuat-kho.js
 * Logic cho Phieu Xuat Kho: Select2 AJAX, grid chi tiet, tinh tien, validate.
 */
var PhieuXuatKho = (function () {
    'use strict';

    function getState($form) {
        if (!$form || $form.length === 0) {
            var $activePane = $('.tab-pane.active');
            $form = $activePane.find('#frmPhieuXuatKho');
            if ($form.length === 0) {
                $form = $('#frmPhieuXuatKho').last();
            }
        }
        var state = $form.data('phieuXuatKhoState');
        if (!state) {
            state = {
                config: {
                    searchKhoUrl: '',
                    searchKhachHangUrl: '',
                    searchNccUrl: '',
                    searchSpUrl: '',
                    searchPhuongTienUrl: '',
                    isReadOnly: false
                }
            };
            $form.data('phieuXuatKhoState', state);
        }
        return state;
    }

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

    function init(cfg) {
        var $form = $('#frmPhieuXuatKho').last();
        var state = getState($form);
        state.config = $.extend(state.config, cfg);

        _initHeaderSelect2($form);
        _initEvents($form);
        _renderTable($form, cfg.chiTietsJson);

        if (state.config.isReadOnly) {
            $form.find('input, select, textarea, button').not('.btn-close, .btn-secondary').prop('disabled', true);
            $form.find('.btn-remove-row, #PXK_btnAddRow, #btnSavePhieu, #btnSaveDraft').hide();
        }
    }

    function _initHeaderSelect2($form) {
        var state = getState($form);

        // Kho xuất
        var $selKho = $form.find('#selKho, [id$="selKho"], .sel-IDKho');
        $selKho.select2({
            placeholder: '-- Chọn kho xuất --',
            allowClear: true,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchKhoUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results || data }; },
                cache: true
            }
        }).on('change select2:select', function () {
            $form.find('#hdIDKho, [name="IDKho"]').val($(this).val() || '');
        }).on('select2:clear', function () {
            $form.find('#hdIDKho, [name="IDKho"]').val('');
        });

        if (state.config.selectedKhoId) {
            if ($selKho.find('option[value="' + state.config.selectedKhoId + '"]').length === 0) {
                $selKho.append(new Option(state.config.selectedKhoText || '', state.config.selectedKhoId, true, true));
            }
            $selKho.val(state.config.selectedKhoId).trigger('change');
            $form.find('#hdIDKho, [name="IDKho"]').val(state.config.selectedKhoId);
        }

        // Khách hàng
        var $selKH = $form.find('#selKhachHang, [id$="selKhachHang"], .sel-IDKhachHang');
        $selKH.select2({
            placeholder: '-- Chọn khách hàng --',
            allowClear: true,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchKhachHangUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results || data }; },
                cache: true
            }
        }).on('change select2:select', function () {
            $form.find('#hdIDKhachHang, [name="IDKhachHang"]').val($(this).val() || '');
        }).on('select2:clear', function () {
            $form.find('#hdIDKhachHang, [name="IDKhachHang"]').val('');
        });

        if (state.config.selectedKhachHangId) {
            if ($selKH.find('option[value="' + state.config.selectedKhachHangId + '"]').length === 0) {
                $selKH.append(new Option(state.config.selectedKhachHangText || '', state.config.selectedKhachHangId, true, true));
            }
            $selKH.val(state.config.selectedKhachHangId).trigger('change');
            $form.find('#hdIDKhachHang, [name="IDKhachHang"]').val(state.config.selectedKhachHangId);
        }

        // Nhà cung cấp
        var $selNCC = $form.find('#selNhaCungCap, [id$="selNhaCungCap"], .sel-IDNhaCungCap');
        $selNCC.select2({
            placeholder: '-- Chọn nhà cung cấp --',
            allowClear: true,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchNccUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results || data }; },
                cache: true
            }
        }).on('change select2:select', function () {
            $form.find('#hdIDNhaCungCap, [name="IDNhaCungCap"]').val($(this).val() || '');
        }).on('select2:clear', function () {
            $form.find('#hdIDNhaCungCap, [name="IDNhaCungCap"]').val('');
        });

        if (state.config.selectedNccId) {
            if ($selNCC.find('option[value="' + state.config.selectedNccId + '"]').length === 0) {
                $selNCC.append(new Option(state.config.selectedNccText || '', state.config.selectedNccId, true, true));
            }
            $selNCC.val(state.config.selectedNccId).trigger('change');
            $form.find('#hdIDNhaCungCap, [name="IDNhaCungCap"]').val(state.config.selectedNccId);
        }

        // Phương tiện
        var $selPT = $form.find('#selPhuongTien, [id$="selPhuongTien"], .sel-IDPhuongTien');
        $selPT.select2({
            placeholder: '-- Chọn phương tiện --',
            allowClear: true,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchPhuongTienUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results || data }; },
                cache: true
            }
        }).on('change select2:select', function () {
            $form.find('#hdIDPhuongTien, [name="IDPhuongTien"]').val($(this).val() || '');
        }).on('select2:clear', function () {
            $form.find('#hdIDPhuongTien, [name="IDPhuongTien"]').val('');
        });

        if (state.config.selectedPhuongTienId) {
            if ($selPT.find('option[value="' + state.config.selectedPhuongTienId + '"]').length === 0) {
                $selPT.append(new Option(state.config.selectedPhuongTienText || '', state.config.selectedPhuongTienId, true, true));
            }
            $selPT.val(state.config.selectedPhuongTienId).trigger('change');
            $form.find('#hdIDPhuongTien, [name="IDPhuongTien"]').val(state.config.selectedPhuongTienId);
        }
    }

    function _renderTable($form, dataList) {
        var $tbody = $form.find('#PXK_tblChiTiet tbody');
        $tbody.empty();

        if (dataList && dataList.length > 0) {
            $.each(dataList, function (i, ct) { _addRowWithData($form, ct); });
        } else {
            _addRowWithData($form, null);
        }
        calcTotal($form);
    }

    function _addRowWithData($form, ct) {
        var state = getState($form);
        var $tbody = $form.find('#PXK_tblChiTiet tbody');
        var id = ct ? ct.ID : 0;
        var soLuong = ct ? ct.SoLuong : '';
        var donGia = ct ? ct.DonGia : '';
        var thue = ct ? ct.ThueGTGT : 0;
        var ghiChu = ct ? ct.GhiChu || '' : '';
        var dvt = ct ? ct.DVT || '' : '';
        var ngaySanXuat = (ct && ct.NgaySanXuat) ? ct.NgaySanXuat.split('T')[0] : '';
        var hanSuDung = (ct && ct.HanSuDung) ? ct.HanSuDung.split('T')[0] : '';

        var actionTd = '';
        if (!state.config.isReadOnly) {
            actionTd = '  <td class="text-center align-middle">' +
                       '    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-row"><i class="bi bi-trash"></i></button>' +
                       '  </td>';
        }

        var html = '<tr data-id="' + id + '">' +
            '  <td class="text-center align-middle row-stt"></td>' +
            '  <td><select class="form-control sel-sanpham" style="width:100%"></select></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-dvt" readonly value="' + dvt + '" /></td>' +
            '  <td><input type="text" class="form-control text-end txt-soluong input-number" value="' + _formatNumber(soLuong) + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            '  <td><input type="text" class="form-control text-end txt-dongia input-number" value="' + _formatNumber(donGia) + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            '  <td><input type="date" class="form-control txt-ngaysanxuat" value="' + ngaySanXuat + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            '  <td><input type="date" class="form-control txt-hansudung" value="' + hanSuDung + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            '  <td><input type="text" class="form-control text-end readonly-cell txt-thanhtien" readonly value="0" /></td>' +
            '  <td class="d-none"><input type="text" class="form-control text-center txt-thue input-number" value="' + thue + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            '  <td class="d-none"><input type="text" class="form-control text-end readonly-cell txt-tienthue" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control text-end readonly-cell txt-tongsauthue" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control txt-ghichu" value="' + ghiChu + '" ' + (state.config.isReadOnly ? 'disabled' : '') + ' /></td>' +
            actionTd +
            '</tr>';

        var $row = $(html);
        $tbody.append($row);

        _initSanPhamSelect2($form, $row, ct);
        calcRow($form, $row);
        _updateSTT($form);
    }

    function _initSanPhamSelect2($form, $row, ctData) {
        var state = getState($form);
        var $select = $row.find('.sel-sanpham');
        $select.select2({
            placeholder: 'Gõ để tìm SP...',
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchSpUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results || data }; },
                cache: true
            }
        });

        if (ctData && ctData.IDSanPham) {
            var text = (ctData.MaSanPham ? ctData.MaSanPham + ' - ' : '') + ctData.TenSanPham;
            $select.append(new Option(text, ctData.IDSanPham, true, true)).trigger('change');
        }

        $select.on('select2:select', function (e) {
            var data = e.params.data;
            $row.find('.txt-dvt').val(data.dvt || '');
            calcRow($form, $row);
        });
    }

    function _initEvents($form) {
        var state = getState($form);
        var $tbody = $form.find('#PXK_tblChiTiet tbody');

        $form.find('#PXK_btnAddRow').off('click').on('click', function () {
            _addRowWithData($form, null);
        });

        $tbody.off('click', '.btn-remove-row').on('click', '.btn-remove-row', function () {
            if ($tbody.find('tr').length <= 1) {
                showToast('warning', 'Phiếu phải có ít nhất 1 chi tiết hàng hóa.');
                return;
            }
            $(this).closest('tr').remove();
            _updateSTT($form);
            calcTotal($form);
        });

        $tbody.off('input change blur', '.txt-soluong, .txt-dongia, .txt-thue').on('input change blur', '.txt-soluong, .txt-dongia, .txt-thue', function (e) {
            var $row = $(this).closest('tr');
            if (e.type === 'blur' || e.type === 'change') {
                if ($(this).hasClass('txt-soluong') || $(this).hasClass('txt-dongia')) {
                    var val = _parseMoney($(this).val());
                    $(this).val(_formatNumber(val));
                }
            }
            calcRow($form, $row);
        });
    }

    function calcRow($form, $row) {
        var soLuong = _parseMoney($row.find('.txt-soluong').val());
        var donGia = _parseMoney($row.find('.txt-dongia').val());
        var thue = _parseMoney($row.find('.txt-thue').val());

        var thanhTien = soLuong * donGia;
        var tienThue = thanhTien * (thue / 100);
        var tongSauThue = thanhTien + tienThue;

        $row.find('.txt-thanhtien').val(_formatNumber(thanhTien));
        $row.find('.txt-tienthue').val(_formatNumber(tienThue));
        $row.find('.txt-tongsauthue').val(_formatNumber(tongSauThue));

        calcTotal($form);
    }

    function _updateSTT($form) {
        var $tbody = $form.find('#PXK_tblChiTiet tbody');
        $tbody.find('tr').each(function (i) {
            $(this).find('.row-stt').text(i + 1);
        });
    }

    function calcTotal($form) {
        var $tbody = $form.find('#PXK_tblChiTiet tbody');
        var tongSoLuong = 0;
        var tongTienHang = 0;
        var tongTienThue = 0;
        var tongCong = 0;

        $tbody.find('tr').each(function () {
            var $row = $(this);
            tongSoLuong += _parseMoney($row.find('.txt-soluong').val());
            tongTienHang += _parseMoney($row.find('.txt-thanhtien').val());
            tongTienThue += _parseMoney($row.find('.txt-tienthue').val());
            tongCong += _parseMoney($row.find('.txt-tongsauthue').val());
        });

        $form.find('#dispTongSoLuong').text(_formatNumber(tongSoLuong));
        $form.find('#dispTongTienHang').text(_formatNumber(tongTienHang));
        $form.find('#dispTongTienThue').text(_formatNumber(tongTienThue));
        $form.find('#dispTongCong').text(_formatNumber(tongCong));
    }

    function validateAndSerialize($form) {
        if (!$form || $form.length === 0) {
            $form = $('#frmPhieuXuatKho').last();
        }
        var isValid = true;
        var errorMsg = '';

        var idKhoVal = $form.find('#hdIDKho, [name="IDKho"]').val() || $form.find('#selKho, .sel-IDKho').val();
        if (!idKhoVal || idKhoVal == '0' || idKhoVal == '') {
            errorMsg += 'Vui lòng chọn Kho xuất.\n';
            isValid = false;
        }

        var chiTiets = [];
        var hasProduct = false;

        $form.find('#PXK_tblChiTiet tbody tr').each(function () {
            var $row = $(this);
            var idSp = parseInt($row.find('.sel-sanpham').val()) || 0;
            var soLuong = _parseMoney($row.find('.txt-soluong').val());
            var donGia = _parseMoney($row.find('.txt-dongia').val());

            if (idSp > 0) {
                hasProduct = true;
                if (soLuong <= 0) {
                    errorMsg += 'Số lượng sản phẩm ở dòng ' + $row.find('.row-stt').text() + ' phải lớn hơn 0.\n';
                    isValid = false;
                }
                chiTiets.push({
                    ID: parseInt($row.attr('data-id')) || 0,
                    IDSanPham: idSp,
                    DVT: $row.find('.txt-dvt').val() || '',
                    SoLuong: soLuong,
                    DonGia: donGia,
                    ThueGTGT: _parseMoney($row.find('.txt-thue').val()),
                    ThanhTien: _parseMoney($row.find('.txt-thanhtien').val()),
                    TienThue: _parseMoney($row.find('.txt-tienthue').val()),
                    TongSauThue: _parseMoney($row.find('.txt-tongsauthue').val()),
                    NgaySanXuat: $row.find('.txt-ngaysanxuat').val() || null,
                    HanSuDung: $row.find('.txt-hansudung').val() || null,
                    GhiChu: $row.find('.txt-ghichu').val() || ''
                });
            }
        });

        if (!hasProduct) {
            errorMsg += 'Vui lòng chọn ít nhất một sản phẩm.\n';
            isValid = false;
        }

        if (!isValid) {
            showToast('error', errorMsg.replace(/\n/g, '<br/>'));
            return null;
        }

        return {
            ID: parseInt($form.find('#ID').val()) || 0,
            SoChungTu: $form.find('#SoChungTu').val(),
            NgayXuat: $form.find('#NgayXuat').val(),
            IDKho: parseInt($form.find('#hdIDKho, [name="IDKho"]').val()) || 0,
            IDKhachHang: parseInt($form.find('#hdIDKhachHang, [name="IDKhachHang"]').val()) || null,
            IDNhaCungCap: parseInt($form.find('#hdIDNhaCungCap, [name="IDNhaCungCap"]').val()) || null,
            IDPhuongTien: parseInt($form.find('#hdIDPhuongTien, [name="IDPhuongTien"]').val()) || null,
            TenNguoiNhan: $form.find('#TenNguoiNhan').val() || '',
            SoDienThoaiNguoiNhan: $form.find('#SoDienThoaiNguoiNhan').val() || '',
            TenNguoiGiao: $form.find('#TenNguoiGiao').val() || '',
            SoDienThoaiNguoiGiao: $form.find('#SoDienThoaiNguoiGiao').val() || '',
            NgayGiaoHang: $form.find('#NgayGiaoHang').val() || null,
            HoTenTaiXe: $form.find('#HoTenTaiXe').val() || '',
            SoDienThoaiTaiXe: $form.find('#SoDienThoaiTaiXe').val() || '',
            SoHoaDon: $form.find('#SoHoaDon').val() || '',
            NgayHoaDon: $form.find('#NgayHoaDon').val() || null,
            GhiChu: $form.find('#GhiChu').val() || '',
            TongTienHang: _parseMoney($form.find('#dispTongTienHang').text()),
            TongTienThue: _parseMoney($form.find('#dispTongTienThue').text()),
            TongCong: _parseMoney($form.find('#dispTongCong').text()),
            ChiTiets: chiTiets
        };
    }

    return {
        init: init,
        validateAndSerialize: validateAndSerialize
    };
})();
