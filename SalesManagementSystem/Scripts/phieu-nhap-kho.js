window.PhieuNhapKhoClass = function (options) {
    var config = $.extend({
        chiTietsJson: [],
        selectedKhoId: null,
        selectedKhoText: '',
        selectedNccId: null,
        selectedNccText: '',
        selectedPhuongTienId: null,
        selectedPhuongTienText: '',
        searchKhoUrl: '',
        searchNccUrl: '',
        searchSpUrl: '',
        searchPhuongTienUrl: '',
        isViewMode: false,
        isInlineDetail: false,
        searchLoaiNhapKhoUrl: '',
        searchKhachHangUrl: '',
        selectedLoaiNhapKhoId: null,
        selectedLoaiNhapKhoText: '',
        selectedKhoNguonId: null,
        selectedKhoNguonText: '',
        selectedKhachHangId: null,
        selectedKhachHangText: ''
    }, options);

    function getEl(selector) {
        if (config.containerSelector) {
            return $(config.containerSelector).find(selector);
        }
        var $active = $('.tab-pane.active');
        if ($active.length > 0) {
            var $el = $active.find(selector);
            if ($el.length > 0) return $el;
        }
        return $(selector);
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

    function _formatDateText(dateStr) {
        if (!dateStr) return '';
        var parts = dateStr.split('-');
        if (parts.length === 3) {
            return parts[2] + '/' + parts[1] + '/' + parts[0];
        }
        return dateStr;
    }

    function init() {
        _initSelect2();
        _initEvents();
        _renderTable(config.chiTietsJson);
    }

    function _initSelect2() {
        if (config.isInlineDetail) return;

        // Load Loại Nhập Kho once and initialize Select2 locally
        $.get(config.searchLoaiNhapKhoUrl, function(res) {
            var $loaiSelect = getEl('.sel-IDLoaiNhapKho');
            // Remove existing options except the empty one if any
            $loaiSelect.find('option[value!=""]').remove();
            
            if (res && res.length > 0) {
                $.each(res, function(i, item) {
                    var opt = new Option(item.text, item.id);
                    // Store 'ma' as a data attribute so change event can read it
                    $(opt).attr('data-ma', item.ma);
                    $loaiSelect.append(opt);
                });
            }
            
            $loaiSelect.select2({
                placeholder: '-- Chọn loại nhập --',
                minimumInputLength: 0
            });
            
            // Re-apply selected value if it exists
            if (config.selectedLoaiNhapKhoId) {
                $loaiSelect.val(config.selectedLoaiNhapKhoId).trigger('change');
            } else {
                // Trigger change to set correct visibility even when no value selected
                $loaiSelect.trigger('change');
            }
        });

        getEl('.sel-IDKhoNguon').select2({
            placeholder: '-- Chọn kho nguồn --',
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

        getEl('.sel-IDKhachHang').select2({
            placeholder: '-- Chọn khách hàng --',
            minimumInputLength: 0,
            ajax: {
                url: config.searchKhachHangUrl,
                dataType: 'json',
                delay: 250,
                data: function (params) { return { q: params.term || '' }; },
                processResults: function (data) { return { results: data }; },
                cache: true
            }
        });

        getEl('.sel-IDKho').select2({
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

        getEl('.sel-IDNhaCungCap').select2({
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

        getEl('.sel-IDPhuongTien').select2({
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
            getEl('.sel-IDKho').append(new Option(config.selectedKhoText, config.selectedKhoId, true, true)).trigger('change');
        }
        if (config.selectedNccId) {
            getEl('.sel-IDNhaCungCap').append(new Option(config.selectedNccText, config.selectedNccId, true, true)).trigger('change');
        }
        if (config.selectedPhuongTienId) {
            getEl('.sel-IDPhuongTien').append(new Option(config.selectedPhuongTienText, config.selectedPhuongTienId, true, true)).trigger('change');
        }

        // IDLoaiNhapKho pre-fill is now handled inside the $.get callback above
        if (config.selectedKhoNguonId) {
            getEl('.sel-IDKhoNguon').append(new Option(config.selectedKhoNguonText, config.selectedKhoNguonId, true, true)).trigger('change');
        }
        if (config.selectedKhachHangId) {
            getEl('.sel-IDKhachHang').append(new Option(config.selectedKhachHangText, config.selectedKhachHangId, true, true)).trigger('change');
        }

        getEl('.sel-IDLoaiNhapKho').on('change', function () {
            var $selectedOpt = $(this).find('option:selected');
            var maLoai = $selectedOpt.attr('data-ma') || '';
            
            // Only use selectedLoaiNhapKhoMa as fallback on initial load (same value as original)
            // Do NOT keep using it after user selects a different option
            if (!maLoai && $(this).val() && config.selectedLoaiNhapKhoId && $(this).val() == config.selectedLoaiNhapKhoId) {
                maLoai = config.selectedLoaiNhapKhoMa || '';
            }
            
            getEl('#colKhoNguon, #colKhachHang, #colNhaCungCap').hide();
            
            if (maLoai === 'CHUYEN_KHO') {
                getEl('#colKhoNguon').show();
                getEl('#colNhaCungCap').show();
            } else if (maLoai === 'TRA_HANG') {
                getEl('#colKhachHang').show();
            } else {
                getEl('#colNhaCungCap').show();
            }
        });
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
        var $tbody = getEl('#PNK_tblChiTiet tbody');
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
        var donGiaVanChuyen = ct ? (ct.DonGiaVanChuyen || 0) : '';
        var thue = ct ? ct.ThueGTGT : 0;
        var ghiChu = ct ? ct.GhiChu || '' : '';
        var dvt = ct ? ct.DVT || '' : '';
        var ngaySanXuat = (ct && ct.NgaySanXuat) ? ct.NgaySanXuat.split('T')[0] : '';
        var hanSuDung = (ct && ct.HanSuDung) ? ct.HanSuDung.split('T')[0] : '';

        if (config.isInlineDetail) {
            var productName = (ct && ct.MaSanPham) ? (ct.MaSanPham + ' - ' + ct.TenSanPham) : '';
            var actionTd = '';
            if (!config.isViewMode) {
                actionTd = '  <td class="text-center align-middle">' +
                           '    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-row"><i class="bi bi-trash"></i></button>' +
                           '  </td>';
            }
            var html = '<tr data-id="' + id + '">' +
                '  <td class="text-center align-middle row-stt"></td>' +
                '  <td class="align-middle">' + productName + '</td>' +
                '  <td class="text-center align-middle">' + dvt + '</td>' +
                '  <td class="text-end align-middle txt-soluong" data-val="' + soLuong + '">' + _formatNumber(soLuong) + '</td>' +
                '  <td class="text-end align-middle txt-dongia" data-val="' + donGia + '">' + _formatNumber(donGia) + '</td>' +
                '  <td class="text-center align-middle">' + _formatDateText(ngaySanXuat) + '</td>' +
                '  <td class="text-center align-middle">' + _formatDateText(hanSuDung) + '</td>' +
                '  <td class="text-end align-middle txt-thanhtien">0</td>' +
                '  <td class="text-end align-middle txt-dongiavanchuyen" data-val="' + donGiaVanChuyen + '">' + _formatNumber(donGiaVanChuyen) + '</td>' +
                '  <td class="text-end align-middle txt-tienvanchuyen">0</td>' +
                '  <td class="d-none txt-thue" data-val="' + thue + '">' + thue + '</td>' +
                '  <td class="d-none txt-tienthue">0</td>' +
                '  <td class="text-end align-middle txt-tongsauthue">0</td>' +
                '  <td class="align-middle">' + ghiChu + '</td>' +
                actionTd +
                '</tr>';

            var $row = $(html);
            $tbody.append($row);
            calcRow($row);
            _updateSTT();
            return;
        }

        var actionTd = '';
        if (!config.isViewMode) {
            actionTd = '  <td class="text-center align-middle">' +
                       '    <button type="button" class="btn btn-sm btn-outline-danger btn-remove-row"><i class="bi bi-trash"></i></button>' +
                       '  </td>';
        }

        var html = '<tr data-id="' + id + '">' +
            '  <td class="text-center align-middle row-stt"></td>' +
            '  <td><select class="form-control sel-sanpham" style="width:100%"></select></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-dvt" readonly value="' + dvt + '" /></td>' +
            '  <td><input type="text" class="form-control txt-soluong text-end" inputmode="numeric" value="' + _formatNumber(soLuong) + '" /></td>' +
            '  <td><input type="text" class="form-control txt-dongia text-end" inputmode="decimal" value="' + _formatNumber(donGia) + '" /></td>' +
            '  <td><input type="date" class="form-control txt-ngaysanxuat" value="' + ngaySanXuat + '" /></td>' +
            '  <td><input type="date" class="form-control txt-hansudung" value="' + hanSuDung + '" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-thanhtien text-end" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control txt-dongiavanchuyen text-end" inputmode="decimal" value="' + _formatNumber(donGiaVanChuyen) + '" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-tienvanchuyen text-end" readonly value="0" /></td>' +
            '  <td class="d-none"><input type="number" class="form-control txt-thue text-end" step="0.1" value="' + thue + '" /></td>' +
            '  <td class="d-none"><input type="text" class="form-control readonly-cell txt-tienthue text-end" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control readonly-cell txt-tongsauthue text-end" readonly value="0" /></td>' +
            '  <td><input type="text" class="form-control txt-ghichu" value="' + ghiChu + '" /></td>' +
            actionTd +
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

        $row.find('.txt-dongia, .txt-thue, .txt-dongiavanchuyen').on('input change', function () {
            calcRow($row);
        });

        $row.find('.txt-dongia, .txt-soluong, .txt-dongiavanchuyen').on('blur change', function () {
            $(this).val(_formatNumber(_parseMoney($(this).val())));
        });

        calcRow($row);
        _updateSTT();
    }

    function _initEvents() {
        getEl('#PNK_btnAddRow').on('click', function () {
            _addRowWithData(getEl('#PNK_tblChiTiet tbody'), null);
        });

        getEl('#PNK_tblChiTiet').on('click', '.btn-remove-row', function () {
            $(this).closest('tr').remove();
            _updateSTT();
            _updateTotal();
        });
    }

    function _updateSTT() {
        getEl('#PNK_tblChiTiet tbody tr').each(function (idx) {
            $(this).find('.row-stt').text(idx + 1);
        });
    }

    function calcRow($row) {
        var slInput = $row.find('.txt-soluong');
        var sl = slInput.is('input') ? _parseMoney(slInput.val()) : _parseMoney(slInput.text() || slInput.attr('data-val'));

        var dgInput = $row.find('.txt-dongia');
        var dg = dgInput.is('input') ? _parseMoney(dgInput.val()) : _parseMoney(dgInput.text() || dgInput.attr('data-val'));

        var thueInput = $row.find('.txt-thue');
        var thuePt = thueInput.is('input') ? (parseFloat(thueInput.val()) || 0) : (parseFloat(thueInput.text() || thueInput.attr('data-val')) || 0);

        var thanhTien = sl * dg;
        var tienThue = thanhTien * thuePt / 100;
        var tong = thanhTien + tienThue;
        var dgvcInput = $row.find('.txt-dongiavanchuyen');
        var dgvc = dgvcInput.is('input') ? _parseMoney(dgvcInput.val()) : _parseMoney(dgvcInput.text() || dgvcInput.attr('data-val'));
        var tienVanChuyen = dgvc * sl;

        var ttInput = $row.find('.txt-thanhtien');
        if (ttInput.is('input')) {
            ttInput.val(_formatNumber(thanhTien));
        } else {
            ttInput.text(_formatNumber(thanhTien));
        }

        var tthInput = $row.find('.txt-tienthue');
        if (tthInput.is('input')) {
            tthInput.val(_formatNumber(tienThue));
        } else {
            tthInput.text(_formatNumber(tienThue));
        }

        var tstInput = $row.find('.txt-tongsauthue');
        if (tstInput.is('input')) {
            tstInput.val(_formatNumber(tong));
        } else {
            tstInput.text(_formatNumber(tong));
        }

        var tvcInput = $row.find('.txt-tienvanchuyen');
        if (tvcInput.is('input')) {
            tvcInput.val(_formatNumber(tienVanChuyen));
        } else {
            tvcInput.text(_formatNumber(tienVanChuyen));
        }

        _updateTotal();
    }

    function _updateTotal() {
        var totalTienHang = 0;
        var totalTienThue = 0;
        var totalCong = 0;
        var totalSoLuong = 0;
        var totalVanChuyen = 0;

        getEl('#PNK_tblChiTiet tbody tr').each(function () {
            var slInput = $(this).find('.txt-soluong');
            var sl = slInput.is('input') ? _parseMoney(slInput.val()) : _parseMoney(slInput.text() || slInput.attr('data-val'));

            var ttInput = $(this).find('.txt-thanhtien');
            var tt = ttInput.is('input') ? _parseMoney(ttInput.val()) : _parseMoney(ttInput.text());

            var tthInput = $(this).find('.txt-tienthue');
            var tth = tthInput.is('input') ? _parseMoney(tthInput.val()) : _parseMoney(tthInput.text());

            var tstInput = $(this).find('.txt-tongsauthue');
            var tst = tstInput.is('input') ? _parseMoney(tstInput.val()) : _parseMoney(tstInput.text());
            var tvcInput = $(this).find('.txt-tienvanchuyen');
            var tvc = tvcInput.is('input') ? _parseMoney(tvcInput.val()) : _parseMoney(tvcInput.text());

            totalSoLuong += sl;
            totalTienHang += tt;
            totalTienThue += tth;
            totalCong += tst;
            totalVanChuyen += tvc;
        });

        getEl('#dispTongSoLuong').text(_formatNumber(totalSoLuong));
        getEl('#dispTongTienHang').text(_formatNumber(totalTienHang));
        getEl('#dispTongTienThue').text(_formatNumber(totalTienThue));
        getEl('#dispTienVanChuyen').text(_formatNumber(totalVanChuyen));
        getEl('#dispTongCong').text(_formatNumber(totalCong));
        getEl('#dispTongCongMini').text(_formatNumber(totalCong));
    }

    function validateAndSerialize() {
        var isValid = true;
        var errorMsg = '';

        if (!getEl('#NgayNhap').val()) { errorMsg += 'Ngày nhập không được để trống.\n'; isValid = false; }
        if (!getEl('.sel-IDLoaiNhapKho').val()) { errorMsg += 'Vui lòng chọn Loại nhập kho.\n'; isValid = false; }
        if (!getEl('.sel-IDKho').val()) { errorMsg += 'Vui lòng chọn Kho.\n'; isValid = false; }
        
        var maLoai = '';
        var $selectedLoai = getEl('.sel-IDLoaiNhapKho').find('option:selected');
        if ($selectedLoai.length > 0) {
            maLoai = $selectedLoai.attr('data-ma') || '';
        } 
        
        if (!maLoai && config.selectedLoaiNhapKhoMa) {
            maLoai = config.selectedLoaiNhapKhoMa;
        }

        if (maLoai === 'CHUYEN_KHO') {
            if (!getEl('.sel-IDKhoNguon').val()) { errorMsg += 'Vui lòng chọn Kho nguồn.\n'; isValid = false; }
            if (getEl('.sel-IDKhoNguon').val() === getEl('.sel-IDKho').val()) { errorMsg += 'Kho nguồn và Kho nhập không được trùng nhau.\n'; isValid = false; }
            if (!getEl('.sel-IDNhaCungCap').val()) { errorMsg += 'Vui lòng chọn Nhà cung cấp.\n'; isValid = false; }
        } else if (maLoai === 'TRA_HANG') {
            if (!getEl('.sel-IDKhachHang').val()) { errorMsg += 'Vui lòng chọn Khách hàng.\n'; isValid = false; }
        } else {
            if (!getEl('.sel-IDNhaCungCap').val()) { errorMsg += 'Vui lòng chọn Nhà cung cấp.\n'; isValid = false; }
        }

        var chiTiets = [];
        var rows = getEl('#PNK_tblChiTiet tbody tr');
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
                var dgvc = _parseMoney($row.find('.txt-dongiavanchuyen').val());

                var rowValid = true;
                if (!spId) { errorMsg += 'Dòng ' + stt + ': Chưa chọn sản phẩm.\n'; rowValid = false; }
                if (sl <= 0) { errorMsg += 'Dòng ' + stt + ': Số lượng phải > 0.\n'; rowValid = false; }
                if (dg < 0) { errorMsg += 'Dòng ' + stt + ': Đơn giá không được âm.\n'; rowValid = false; }
                if (thue < 0) { errorMsg += 'Dòng ' + stt + ': Thuế GTGT không được âm.\n'; rowValid = false; }
                
                if (dgvc < 0) { errorMsg += 'Dong ' + stt + ': Don gia van chuyen khong duoc am.\n'; rowValid = false; }

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
                        DonGiaVanChuyen: dgvc,
                        TienVanChuyen: dgvc * sl,
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
            ID: getEl('#ID').val(),
            SoChungTu: getEl('#SoChungTu').val(),
            NgayNhap: getEl('#NgayNhap').val(),
            IDKho: getEl('.sel-IDKho').val(),
            IDNhaCungCap: getEl('.sel-IDNhaCungCap').val(),
            SoHoaDon: getEl('#SoHoaDon').val(),
            NgayHoaDon: getEl('#NgayHoaDon').val(),
            TenNguoiGiao: getEl('#TenNguoiGiao').val(),
            SoDienThoaiNguoiGiao: getEl('#SoDienThoaiNguoiGiao').val(),
            TenNguoiNhan: getEl('#TenNguoiNhan').val(),
            GhiChu: getEl('#GhiChu').val(),
            IDPhuongTien: getEl('.sel-IDPhuongTien').val(),
            NgayGiaoHang: getEl('#NgayGiaoHang').val(),
            HoTenTaiXe: getEl('#HoTenTaiXe').val(),
            SoDienThoaiTaiXe: getEl('#SoDienThoaiTaiXe').val(),
            IDLoaiNhapKho: getEl('.sel-IDLoaiNhapKho').val(),
            IDKhoNguon: getEl('.sel-IDKhoNguon').val(),
            IDKhachHang: getEl('.sel-IDKhachHang').val(),
            ChiTiets: chiTiets
        };
    }

    this.init = init;
    this.validateAndSerialize = validateAndSerialize;
};
