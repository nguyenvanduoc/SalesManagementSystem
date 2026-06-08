/**
 * don-dat-hang.js
 * Logic cho form Đơn Đặt Hàng: Select2 AJAX, grid chi tiết, tính tiền realtime.
 */
var DonDatHang = (function () {
    'use strict';

    var _config = {
        searchKhUrl: '',
        searchSpUrl: ''
    };

    var _rowIndex = 0; // Đảm bảo ID dòng luôn unique

    // ── Khởi tạo ──────────────────────────────────────────────────────────────

    function init(cfg) {
        _config = $.extend(_config, cfg);

        _initKhachHangSelect2();

        // Pre-fill KH khi Edit (selectedKhId/selectedKhText được truyền từ server)
        if (cfg.selectedKhId && cfg.selectedKhId != 'null' && cfg.selectedKhId != null) {
            var option = new Option(cfg.selectedKhText || '', cfg.selectedKhId, true, true);
            $('#selKhachHang').append(option).trigger('change');
            $('#hdIDKhachHang').val(cfg.selectedKhId);
        }

        // Render các dòng chi tiết đã có (khi Edit)
        var existing = cfg.chiTietsJson;
        if (existing && Array.isArray(existing) && existing.length > 0) {
            existing.forEach(function (ct) { _addRowWithData(ct); });
        } else {
            addRow(); // Thêm 1 dòng trống mặc định
        }

        calcTotal();
    }

    // ── Select2: Khách hàng ───────────────────────────────────────────────────

    function _initKhachHangSelect2() {
        $('#selKhachHang').select2({
            placeholder:    'Tìm theo mã, tên, SĐT, MST...',
            allowClear:     true,
            minimumInputLength: 0,
            width:          '100%',
            ajax: {
                url:            _config.searchKhUrl,
                dataType:       'json',
                delay:          250,
                data:           function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data.results }; },
                cache:          true
            },
            templateResult:   _formatKhachHangOption,
            templateSelection: function (d) { return d.text || d.id; }
        }).on('select2:select', function (e) {
            var d = e.params.data;
            $('#hdIDKhachHang').val(d.id);
            $('#txtMaKH').val(d.maKH      || '');
            $('#txtMaSoThue').val(d.maSoThue || '');
            $('#txtDiaChi').val(d.diaChi    || '');
            $('#txtSDT').val(d.sdt          || '');
            // Gán nhân viên mặc định nếu có
            if (d.idNhanVien) {
                var $selNV = $('#selNhanVien');
                if ($selNV.find('option[value="' + d.idNhanVien + '"]').length > 0) {
                    $selNV.val(d.idNhanVien);
                }
            }
        }).on('select2:clear', function () {
            $('#hdIDKhachHang').val('');
            $('#txtMaKH,#txtMaSoThue,#txtDiaChi,#txtSDT').val('');
        });
    }

    function _formatKhachHangOption(d) {
        if (!d.id) return d.text;
        return $('<div class="py-1">' +
            '<div class="fw-bold">' + (d.text || '') + '</div>' +
            '<small class="text-muted">MST: ' + (d.maSoThue || '—') +
            ' | SĐT: ' + (d.sdt || '—') + '</small></div>');
    }

    // ── Grid chi tiết ─────────────────────────────────────────────────────────

    function addRow() {
        _addRowWithData(null);
    }

    function _addRowWithData(ct) {
        var idx = _rowIndex++;
        var html =
            '<tr data-idx="' + idx + '">' +
            '  <td class="text-center stt-cell"></td>' +
            '  <td>' +
            '    <select class="form-select sel-sp" id="selSP_' + idx + '" style="width:100%;min-width:180px;"></select>' +
            '    <input type="hidden" class="hd-idsp" value="" />' +
            '  </td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-masp" readonly placeholder="-" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-tensp" readonly placeholder="-" /></td>' +
            '  <td class="text-center"><input type="text" class="form-control readonly-cell txt-dvt" readonly placeholder="-" style="text-align:center;" /></td>' +
            '  <td class="text-center">' +
            '    <input type="number" class="form-control txt-thue" min="0" max="100" step="0.01" value="' + (ct ? ct.thueGTGT || 0 : 0) + '" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="number" class="form-control txt-dongia text-end" min="0" step="1000" value="' + (ct ? ct.donGia || 0 : 0) + '" />' +
            '  </td>' +
            '  <td>' +
            '    <input type="text" class="form-control readonly-cell txt-thanhtien text-end" readonly value="0" />' +
            '  </td>' +
            '  <td class="text-center">' +
            '    <input type="checkbox" class="form-check-input chk-km" ' + (ct && ct.isHangKhuyenMai ? 'checked' : '') + ' />' +
            '  </td>' +
            '  <td><input type="text" class="form-control txt-ghichu" value="' + _escape(ct ? ct.ghiChu || '' : '') + '" /></td>' +
            '  <td class="text-center">' +
            '    <button type="button" class="btn btn-sm btn-outline-danger" onclick="DonDatHang.removeRow(this)" title="Xóa dòng">' +
            '      <i class="bi bi-trash3"></i>' +
            '    </button>' +
            '  </td>' +
            '</tr>';

        var $row = $(html);
        $('#tbodyChiTiet').append($row);

        // Select2 cho sản phẩm
        _initSanPhamSelect2($row, ct);

        // Bind sự kiện tính tiền
        $row.find('.txt-dongia, .txt-thue').on('input change', function () {
            calcRow($row);
        });

        // Render STT và tổng
        _updateSTT();
        calcTotal();
    }

    function _initSanPhamSelect2($row, ct) {
        var $sel = $row.find('.sel-sp');

        // Nếu có dữ liệu sẵn thì thêm option
        if (ct && ct.idSanPham) {
            var optText = (ct.maSanPham || '') + ' - ' + (ct.tenSanPham || '');
            $sel.append(new Option(optText, ct.idSanPham, true, true));
            $row.find('.hd-idsp').val(ct.idSanPham);
            $row.find('.txt-masp').val(ct.maSanPham || '');
            $row.find('.txt-tensp').val(ct.tenSanPham || '');
            $row.find('.txt-dvt').val(ct.dvt || '');
        }

        $sel.select2({
            placeholder:       'Tìm sản phẩm...',
            allowClear:        true,
            minimumInputLength:0,
            width:             '100%',
            dropdownParent:    $('body'),
            ajax: {
                url:            _config.searchSpUrl,
                dataType:       'json',
                delay:          250,
                data:           function (p) { return { q: p.term || '' }; },
                processResults: function (d) { return { results: d.results }; },
                cache:          true
            }
        }).on('select2:select', function (e) {
            var d    = e.params.data;
            var $row = $(this).closest('tr');
            $row.find('.hd-idsp').val(d.id);
            $row.find('.txt-masp').val(d.maSanPham  || '');
            $row.find('.txt-tensp').val(d.tenSanPham || '');
            $row.find('.txt-dvt').val(d.dvt          || '');
        }).on('select2:clear', function () {
            var $row = $(this).closest('tr');
            $row.find('.hd-idsp,.txt-masp,.txt-tensp,.txt-dvt').val('');
        });

        // Tính lại sau khi có dữ liệu
        if (ct) calcRow($row);
    }

    function removeRow(btn) {
        $(btn).closest('tr').remove();
        _updateSTT();
        calcTotal();
    }

    function _updateSTT() {
        $('#tbodyChiTiet tr').each(function (i) {
            $(this).find('.stt-cell').text(i + 1);
        });
        $('#dispSoDong').text($('#tbodyChiTiet tr').length);
    }

    // ── Tính tiền ─────────────────────────────────────────────────────────────

    /**
     * ThanhTien = DonGia + DonGia * ThueGTGT / 100
     */
    function calcRow($row) {
        var donGia   = parseFloat($row.find('.txt-dongia').val())  || 0;
        var thue     = parseFloat($row.find('.txt-thue').val())    || 0;
        var thanhTien = donGia + (donGia * thue / 100);
        $row.find('.txt-thanhtien').val(_formatNumber(Math.round(thanhTien)));
        calcTotal();
    }

    function calcTotal() {
        var total = 0;
        $('#tbodyChiTiet tr').each(function () {
            var v = parseFloat($(this).find('.txt-thanhtien').val().replace(/,/g, '')) || 0;
            total += v;
        });
        var formatted = _formatNumber(Math.round(total));
        $('#dispTongTien, #dispTongTien2').text(formatted);
        $('#dispSoDong').text($('#tbodyChiTiet tr').length);
    }

    // ── Validate & Serialize ───────────────────────────────────────────────────

    function validateAndSerialize() {
        var ok = true;

        // Validate khách hàng
        var idKH = $('#hdIDKhachHang').val();
        if (!idKH || idKH === '0') {
            $('#selKhachHang').next('.select2').find('.select2-selection')
                .css('border-color', '#dc3545');
            showToast('warning', 'Vui lòng chọn khách hàng.');
            ok = false;
        } else {
            $('#selKhachHang').next('.select2').find('.select2-selection')
                .css('border-color', '');
        }

        // Validate nhân viên
        if (!$('#selNhanVien').val()) {
            $('#selNhanVien').addClass('field-error');
            if (ok) showToast('warning', 'Vui lòng chọn nhân viên phụ trách.');
            ok = false;
        } else {
            $('#selNhanVien').removeClass('field-error');
        }

        // Validate số đơn hàng
        if (!$('#SoDonHang').val().trim()) {
            $('#SoDonHang').addClass('field-error');
            if (ok) showToast('warning', 'Vui lòng nhập số đơn hàng.');
            ok = false;
        } else {
            $('#SoDonHang').removeClass('field-error');
        }

        // Validate chi tiết
        var rows = $('#tbodyChiTiet tr');
        if (rows.length === 0) {
            $('#validChiTiet').show();
            if (ok) showToast('warning', 'Vui lòng thêm ít nhất một sản phẩm.');
            ok = false;
        } else {
            $('#validChiTiet').hide();
        }

        if (!ok) return false;

        // Serialize chi tiết thành JSON
        var chiTiets = [];
        rows.each(function () {
            var $r = $(this);
            chiTiets.push({
                idSanPham:       parseInt($r.find('.hd-idsp').val())     || 0,
                maSanPham:       $r.find('.txt-masp').val(),
                tenSanPham:      $r.find('.txt-tensp').val(),
                dvt:             $r.find('.txt-dvt').val(),
                soLuong:         1,
                donGia:          parseFloat($r.find('.txt-dongia').val())    || 0,
                thueGTGT:        parseFloat($r.find('.txt-thue').val())      || 0,
                thanhTien:       parseFloat($r.find('.txt-thanhtien').val().replace(/,/g,'')) || 0,
                isHangKhuyenMai: $r.find('.chk-km').is(':checked'),
                ghiChu:          $r.find('.txt-ghichu').val()
            });
        });

        $('#hdChiTietsJson').val(JSON.stringify(chiTiets));
        return true;
    }

    // ── Utils ─────────────────────────────────────────────────────────────────

    function _formatNumber(n) {
        if (isNaN(n)) return '0';
        return n.toLocaleString('vi-VN');
    }

    function _escape(str) {
        if (!str) return '';
        return str.replace(/"/g, '&quot;').replace(/'/g, '&#39;');
    }

    // ── Public API ────────────────────────────────────────────────────────────

    return {
        init:               init,
        addRow:             addRow,
        removeRow:          removeRow,
        calcRow:            calcRow,
        calcTotal:          calcTotal,
        validateAndSerialize: validateAndSerialize
    };
})();
