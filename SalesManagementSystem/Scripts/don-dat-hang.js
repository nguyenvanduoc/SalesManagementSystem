/**
 * don-dat-hang.js
 * Logic cho form Don Dat Hang: Select2 AJAX, grid chi tiet, tinh tien realtime.
 */
var DonDatHang = (function () {
    'use strict';

    function getState($form) {
        if (!$form || $form.length === 0) {
            var $activePane = $('.tab-pane.active');
            $form = $activePane.find('#frmDonDatHang');
            if ($form.length === 0) {
                $form = $('#frmDonDatHang').last();
            }
        }
        var state = $form.data('donDatHangState');
        if (!state) {
            state = {
                config: {
                    searchKhUrl: '',
                    searchSpUrl: '',
                    isReadOnly: false
                },
                rowIndex: 0
            };
            $form.data('donDatHangState', state);
        }
        return state;
    }

    function init(cfg) {
        var $form = $('#frmDonDatHang').last();
        var state = getState($form);
        state.config = $.extend(state.config, cfg);

        _initKhachHangSelect2($form);

        if (cfg.selectedKhId && cfg.selectedKhId != 'null' && cfg.selectedKhId != null) {
            var option = new Option(cfg.selectedKhText || '', cfg.selectedKhId, true, true);
            $form.find('#selKhachHang').append(option).trigger('change');
            $form.find('#hdIDKhachHang').val(cfg.selectedKhId);
        }

        var existing = cfg.chiTietsJson;
        if (existing && Array.isArray(existing) && existing.length > 0) {
            existing.forEach(function (ct) { _addRowWithData($form, ct); });
        } else {
            _addRowWithData($form, null);
        }

        $form.find('#PhiBocXepDisplay').on('input change blur', function (e) {
            var val = $(this).val();
            if (e.type === 'blur' || e.type === 'change') {
                $(this).val(_formatNumber(_parseMoney(val)));
            }
            $form.find('#PhiBocXep').val(_parseMoney(val));
            calcTotal($form);
        });

        calcTotal($form);
    }

    function _initKhachHangSelect2($form) {
        var state = getState($form);
        $form.find('#selKhachHang').select2({
            placeholder: 'Tìm theo mã, tên, SĐT, MST...',
            allowClear: !state.config.isReadOnly,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            ajax: {
                url: state.config.searchKhUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results }; },
                cache: true
            },
            templateResult: _formatKhachHangOption,
            templateSelection: function (d) { return d.text || d.id; }
        }).on('select2:select', function (e) {
            var d = e.params.data;
            $form.find('#hdIDKhachHang').val(d.id);
            $form.find('#txtMaKH').val(d.maKH || '');
            $form.find('#txtMaSoThue').val(d.maSoThue || '');
            $form.find('#txtDiaChi').val(d.diaChi || '');
            $form.find('#txtSDT').val(d.sdt || '');

            if (d.idNhanVien) {
                var $selNV = $form.find('#selNhanVien');
                if ($selNV.find('option[value="' + d.idNhanVien + '"]').length > 0) {
                    $selNV.val(d.idNhanVien);
                }
            }
        }).on('select2:clear', function () {
            $form.find('#hdIDKhachHang').val('');
            $form.find('#txtMaKH,#txtMaSoThue,#txtDiaChi,#txtSDT').val('');
        });
    }

    function _formatKhachHangOption(d) {
        if (!d.id) return d.text;
        return $('<div class="py-1">' +
            '<div class="fw-bold">' + (d.text || '') + '</div>' +
            '<small class="text-muted">MST: ' + (d.maSoThue || '—') +
            ' | SĐT: ' + (d.sdt || '—') + '</small></div>');
    }

    function addRow(formOrBtn) {
        var $form;
        if (formOrBtn && $(formOrBtn).is('form')) {
            $form = $(formOrBtn);
        } else if (formOrBtn) {
            $form = $(formOrBtn).closest('form');
        } else {
            $form = $('.tab-pane.active').find('#frmDonDatHang');
            if ($form.length === 0) $form = $('#frmDonDatHang').last();
        }
        _addRowWithData($form, null);
    }

    function _addRowWithData($form, ct) {
        var state = getState($form);
        var idx = state.rowIndex++;
        var id = parseInt(_val(ct, 'id', 'ID')) || 0;
        var thueGTGT = _toNumber(_val(ct, 'thueGTGT', 'ThueGTGT'), 0);
        var donGia = _toNumber(_val(ct, 'donGia', 'DonGia'), 0);
        var soLuong = _toNumber(_val(ct, 'soLuong', 'SoLuong'), 1);
        var isHangKhuyenMai = !!_val(ct, 'isHangKhuyenMai', 'IsHangKhuyenMai');
        var ghiChu = _val(ct, 'ghiChu', 'GhiChu') || '';

        if (soLuong < 0) soLuong = 1;

        var html =
            '<tr data-idx="' + idx + '">' +
            '  <td class="text-center stt-cell"><input type="hidden" class="hd-idct" value="' + id + '" /></td>' +
            '  <td>' +
            '    <select class="form-select sel-sp" id="selSP_' + idx + '" style="width:100%;min-width:180px;"></select>' +
            '    <input type="hidden" class="hd-idsp" value="" />' +
            '    <input type="hidden" class="txt-masp" value="" />' +
            '    <input type="hidden" class="txt-tensp" value="" />' +
            '  </td>' +
            '  <td class="text-center"><input type="text" class="form-control readonly-cell txt-dvt" readonly placeholder="-" style="text-align:center;" /></td>' +
            '  <td>' +
            '    <input type="text" class="form-control txt-soluong text-end" inputmode="numeric" value="' + _formatNumber(Math.round(soLuong)) + '" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="text" class="form-control txt-dongia text-end" inputmode="decimal" value="' + _formatNumber(donGia) + '" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="text" class="form-control readonly-cell txt-thanhtien text-end" readonly value="0" />' +
            '  </td>' +
            '  <td class="text-center">' +
            '    <input type="number" class="form-control txt-thue" min="0" step="0.01" value="' + thueGTGT + '" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="text" class="form-control readonly-cell txt-tien-thue text-end" readonly value="0" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="text" class="form-control readonly-cell txt-tt-sau-thue text-end" readonly value="0" />' +
            '  </td>' +
            '  <td class="text-center">' +
            '    <input type="checkbox" class="form-check-input chk-km" ' + (isHangKhuyenMai ? 'checked' : '') + ' />' +
            '  </td>' +
            '  <td><input type="text" class="form-control txt-ghichu" value="' + _escape(ghiChu) + '" /></td>' +
            '  <td class="text-center">' +
            '    <button type="button" class="btn btn-sm btn-outline-danger btn-remove" onclick="DonDatHang.removeRow(this)" title="Xóa dòng">' +
            '      <i class="bi bi-trash3"></i>' +
            '    </button>' +
            '  </td>' +
            '</tr>';

        var $row = $(html);
        $form.find('#tbodyChiTiet').append($row);

        _initSanPhamSelect2($form, $row, ct);

        $row.find('.txt-soluong').on('input change', function () {
            var val = $(this).val();
            var sanitized = val.replace(/[^0-9.]/g, '');
            if (val !== sanitized) {
                $(this).val(sanitized);
            }
            calcRow($row);
        });

        $row.find('.txt-dongia, .txt-thue').on('input change', function () {
            calcRow($row);
        });
        $row.find('.txt-dongia, .txt-soluong').on('blur change', function () {
            $(this).val(_formatNumber(_parseMoney($(this).val())));
        });

        if (state.config.isReadOnly) {
            $row.find('.txt-dongia, .txt-soluong, .txt-thue, .txt-ghichu, .chk-km').prop('disabled', true);
            $row.find('.btn-remove').remove();
        }

        _updateSTT($form);
        calcRow($row);
    }

    function _initSanPhamSelect2($form, $row, ct) {
        var state = getState($form);
        var $sel = $row.find('.sel-sp');
        var idSanPham = _val(ct, 'idSanPham', 'IDSanPham');

        if (ct && idSanPham) {
            var maSanPham = _val(ct, 'maSanPham', 'MaSanPham') || '';
            var tenSanPham = _val(ct, 'tenSanPham', 'TenSanPham') || '';
            var dvt = _val(ct, 'dvt', 'DVT') || '';
            $sel.append(new Option(maSanPham + ' - ' + tenSanPham, idSanPham, true, true));
            $row.find('.hd-idsp').val(idSanPham);
            $row.find('.txt-masp').val(maSanPham);
            $row.find('.txt-tensp').val(tenSanPham);
            $row.find('.txt-dvt').val(dvt);
        }

        $sel.select2({
            placeholder: 'Tìm sản phẩm...',
            allowClear: !state.config.isReadOnly,
            disabled: state.config.isReadOnly,
            minimumInputLength: 0,
            width: '100%',
            dropdownParent: $form,
            ajax: {
                url: state.config.searchSpUrl,
                dataType: 'json',
                delay: 250,
                data: function (p) { return { q: p.term || '' }; },
                processResults: function (d) { return { results: d.results }; },
                cache: true
            }
        }).on('select2:select', function (e) {
            var d = e.params.data;
            var $row = $(this).closest('tr');
            $row.find('.hd-idsp').val(d.id);
            $row.find('.txt-masp').val(d.maSanPham || '');
            $row.find('.txt-tensp').val(d.tenSanPham || '');
            $row.find('.txt-dvt').val(d.dvt || '');
            if (d.donGia !== undefined && d.donGia !== null) {
                $row.find('.txt-dongia').val(_formatNumber(_parseMoney(d.donGia)));
            }
            if (d.thueGTGT !== undefined && d.thueGTGT !== null) {
                $row.find('.txt-thue').val(d.thueGTGT);
            }
            calcRow($row);
        }).on('select2:clear', function () {
            var $row = $(this).closest('tr');
            $row.find('.hd-idsp,.txt-masp,.txt-tensp,.txt-dvt').val('');
            calcRow($row);
        });
    }

    function removeRow(btn) {
        var $row = $(btn).closest('tr');
        var $form = $row.closest('form');
        $row.remove();
        _updateSTT($form);
        calcTotal($form);
    }

    function _updateSTT($form) {
        $form.find('#tbodyChiTiet tr').each(function (i) {
            var $cell = $(this).find('.stt-cell');
            var $id = $cell.find('.hd-idct').detach();
            $cell.text(i + 1);
            $cell.append($id);
        });
        $form.find('#dispSoDong').text($form.find('#tbodyChiTiet tr').length);
    }

    function calcRow($row) {
        var $form = $row.closest('form');
        var donGia = _parseMoney($row.find('.txt-dongia').val());
        var soLuong = _parseMoney($row.find('.txt-soluong').val());
        var thue = _toNumber($row.find('.txt-thue').val(), 0);

        if (donGia < 0) donGia = 0;
        if (soLuong < 0) soLuong = 0;
        if (thue < 0) thue = 0;

        var thanhTien = donGia * soLuong;
        var tienThue = thanhTien * thue / 100;
        var ttSauThue = thanhTien + tienThue;
        $row.find('.txt-thanhtien').val(_formatNumber(Math.round(thanhTien)));
        $row.find('.txt-tien-thue').val(_formatNumber(Math.round(tienThue)));
        $row.find('.txt-tt-sau-thue').val(_formatNumber(Math.round(ttSauThue)));
        calcTotal($form);
    }

    function calcTotal($form) {
        if (!$form || !$form.jquery) {
            $form = $('.tab-pane.active').find('#frmDonDatHang');
            if ($form.length === 0) $form = $('#frmDonDatHang').last();
        }
        var totalTienHang = 0;
        var totalTienThue = 0;
        $form.find('#tbodyChiTiet tr').each(function () {
            var valThanhTien = $(this).find('.txt-thanhtien').val() || '0';
            var valTienThue = $(this).find('.txt-tien-thue').val() || '0';
            totalTienHang += _parseMoney(valThanhTien);
            totalTienThue += _parseMoney(valTienThue);
        });
        
        var phiBocXep = _parseMoney($form.find('#PhiBocXepDisplay').val() || '0');
        var totalThanhToan = totalTienHang + totalTienThue - phiBocXep;

        $form.find('#dispTongTienHang').text(_formatNumber(Math.round(totalTienHang)));
        $form.find('#dispTongTienThue').text(_formatNumber(Math.round(totalTienThue)));
        $form.find('#dispTongTien, #dispTongTien2').text(_formatNumber(Math.round(totalThanhToan)));
        $form.find('#dispSoDong').text($form.find('#tbodyChiTiet tr').length);
    }

    function validateAndSerialize($form) {
        if (!$form || !$form.jquery) {
            $form = $('.tab-pane.active').find('#frmDonDatHang');
            if ($form.length === 0) $form = $('#frmDonDatHang').last();
        }
        var ok = true;

        var idKH = $form.find('#hdIDKhachHang').val();
        if (!idKH || idKH === '0') {
            $form.find('#selKhachHang').next('.select2').find('.select2-selection')
                .css('border-color', '#dc3545');
            showToast('warning', 'Vui lòng chọn khách hàng.');
            ok = false;
        } else {
            $form.find('#selKhachHang').next('.select2').find('.select2-selection')
                .css('border-color', '');
        }

        if (!$form.find('#selNhanVien').val()) {
            $form.find('#selNhanVien').addClass('field-error');
            if (ok) showToast('warning', 'Vui lòng chọn nhân viên phụ trách.');
            ok = false;
        } else {
            $form.find('#selNhanVien').removeClass('field-error');
        }

        if (!$form.find('#SoDonHang').val().trim()) {
            $form.find('#SoDonHang').addClass('field-error');
            if (ok) showToast('warning', 'Vui lòng nhập số đơn hàng.');
            ok = false;
        } else {
            $form.find('#SoDonHang').removeClass('field-error');
        }

        var rows = $form.find('#tbodyChiTiet tr');
        if (rows.length === 0) {
            $form.find('#validChiTiet').show();
            if (ok) showToast('warning', 'Vui lòng thêm ít nhất một sản phẩm.');
            ok = false;
        } else {
            $form.find('#validChiTiet').hide();
        }

        if (!ok) return false;

        var chiTiets = [];
        rows.each(function () {
            var $r = $(this);
            var thanhTien = $r.find('.txt-thanhtien').val() || '0';
            var tienThue = $r.find('.txt-tien-thue').val() || '0';
            var ttSauThue = $r.find('.txt-tt-sau-thue').val() || '0';
            chiTiets.push({
                id: parseInt($r.find('.hd-idct').val()) || 0,
                idSanPham: parseInt($r.find('.hd-idsp').val()) || 0,
                maSanPham: $r.find('.txt-masp').val(),
                tenSanPham: $r.find('.txt-tensp').val(),
                dvt: $r.find('.txt-dvt').val(),
                soLuong: _parseMoney($r.find('.txt-soluong').val()),
                donGia: _parseMoney($r.find('.txt-dongia').val()),
                thueGTGT: _toNumber($r.find('.txt-thue').val(), 0),
                thanhTien: _toNumber(thanhTien.replace(/,/g, '').replace(/\./g, ''), 0),
                thanhTienThue: _toNumber(tienThue.replace(/,/g, '').replace(/\./g, ''), 0),
                thanhTienSauThue: _toNumber(ttSauThue.replace(/,/g, '').replace(/\./g, ''), 0),
                isHangKhuyenMai: $r.find('.chk-km').is(':checked'),
                ghiChu: $r.find('.txt-ghichu').val()
            });
        });

        $form.find('#hdChiTietsJson').val(JSON.stringify(chiTiets));
        return true;
    }

    function _formatNumber(n) {
        if (isNaN(n)) return '0';
        return n.toLocaleString('vi-VN');
    }

    function _toNumber(value, defaultValue) {
        var n = parseFloat(value);
        return isNaN(n) ? defaultValue : n;
    }

    function _parseMoney(value) {
        if (value === undefined || value === null) return 0;
        var normalized = String(value).replace(/[^\d-]/g, '');
        var n = parseFloat(normalized);
        return isNaN(n) ? 0 : n;
    }

    function _val(obj, camelName, pascalName) {
        if (!obj) return undefined;
        if (obj[camelName] !== undefined && obj[camelName] !== null) return obj[camelName];
        return obj[pascalName];
    }

    function _escape(str) {
        if (!str) return '';
        return String(str).replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    return {
        init: init,
        addRow: addRow,
        removeRow: removeRow,
        calcRow: calcRow,
        calcTotal: calcTotal,
        validateAndSerialize: validateAndSerialize
    };
})();
