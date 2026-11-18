/**
 * Chọn chủ đề màu menu (sidebar + topbar) từ các mẫu thiết kế sẵn.
 *
 * - Màu của từng chủ đề nằm ở Content/css/menu-theme.css (ghi đè biến CSS).
 * - File này chỉ giữ danh sách chủ đề để hiển thị bản xem trước trong hộp chọn.
 * - Chủ đề được lưu ở localStorage, khóa riêng theo tài khoản (window.MENU_THEME_KEY,
 *   do _Layout.cshtml sinh ra) và được áp ngay trong <head> nên không bị nháy màu.
 */
var MenuTheme = (function () {
    // Mã chủ đề phải trùng với html[data-menu-theme="..."] trong menu-theme.css.
    // preview: [topbar, nền menu, mục đang chọn, chữ menu]
    var THEMES = [
        { id: 'default',  name: 'Xanh biển (mặc định)', preview: ['#17202a', '#1a5276', '#148f77', '#d6eaf8'] },
        { id: 'navy',     name: 'Xanh đêm',             preview: ['#0a1628', '#0f2341', '#2f6fde', '#c9d6ea'] },
        { id: 'green',    name: 'Xanh lá',              preview: ['#0b2e1b', '#14532d', '#15803d', '#d1fae5'] },
        { id: 'purple',   name: 'Tím',                  preview: ['#22113a', '#3b1f5e', '#6d3fc0', '#e9ddff'] },
        { id: 'maroon',   name: 'Đỏ đô',                preview: ['#3d0f17', '#6b1d2a', '#b91c1c', '#fde2e4'] },
        { id: 'amber',    name: 'Cam đất',              preview: ['#431407', '#7c2d12', '#c2410c', '#ffedd5'] },
        { id: 'graphite', name: 'Than chì',             preview: ['#12181f', '#1f2933', '#2f6f9f', '#d3dae1'] },
        { id: 'light',    name: 'Sáng',                 preview: ['#0b5b84', '#ffffff', '#1a73e8', '#2c3e50'] }
    ];
    var MODAL_ID = 'menuThemeModal';

    var savedTheme = null;   // chủ đề đang lưu (để Hủy thì trả lại)
    var pendingTheme = null; // chủ đề đang xem thử trong hộp chọn

    function storageKey() {
        return window.MENU_THEME_KEY || 'menuTheme_guest';
    }

    function isValid(id) {
        return THEMES.some(function (t) { return t.id === id; });
    }

    /** Đọc chủ đề đã lưu; không có hoặc không hợp lệ -> 'default'. */
    function getSaved() {
        var id = null;
        try { id = localStorage.getItem(storageKey()); } catch (e) { }
        return isValid(id) ? id : 'default';
    }

    /** Áp chủ đề lên giao diện (chỉ đổi thuộc tính data-menu-theme, chưa lưu). */
    function apply(id) {
        if (!isValid(id)) id = 'default';
        if (id === 'default') {
            document.documentElement.removeAttribute('data-menu-theme');
        } else {
            document.documentElement.setAttribute('data-menu-theme', id);
        }
    }

    /** Lưu chủ đề cho tài khoản hiện tại trên trình duyệt này. */
    function save(id) {
        try {
            if (id === 'default') localStorage.removeItem(storageKey());
            else localStorage.setItem(storageKey(), id);
            return true;
        } catch (e) {
            return false;
        }
    }

    function renderCards() {
        return THEMES.map(function (t) {
            var p = t.preview;
            return '' +
                '<button type="button" class="menu-theme-card" data-theme="' + t.id + '" aria-pressed="false">' +
                '  <div class="menu-theme-preview">' +
                '    <div class="p-top" style="background:' + p[0] + '"></div>' +
                '    <div class="p-side" style="background:' + p[1] + '">' +
                '      <i style="background:' + p[3] + '"></i>' +
                '      <i class="on" style="background:' + p[2] + '"></i>' +
                '      <i style="background:' + p[3] + '"></i>' +
                '      <i style="background:' + p[3] + '"></i>' +
                '    </div>' +
                '    <div class="p-main"></div>' +
                '  </div>' +
                '  <div class="menu-theme-name"><span>' + t.name + '</span><i class="bi bi-check-circle-fill"></i></div>' +
                '</button>';
        }).join('');
    }

    function ensureModal() {
        var el = document.getElementById(MODAL_ID);
        if (el) return el;

        var html = '' +
            '<div class="modal fade" id="' + MODAL_ID + '" tabindex="-1" aria-labelledby="' + MODAL_ID + 'Title" aria-hidden="true" data-bs-backdrop="static" data-bs-keyboard="false">' +
            '  <div class="modal-dialog modal-lg modal-dialog-centered">' +
            '    <div class="modal-content">' +
            '      <div class="modal-header">' +
            '        <h5 class="modal-title fw-bold" id="' + MODAL_ID + 'Title" style="color:#0b5b84;"><i class="bi bi-palette me-2"></i>Chọn màu menu</h5>' +
            '        <button type="button" class="btn-close" data-menu-theme-cancel aria-label="Đóng"></button>' +
            '      </div>' +
            '      <div class="modal-body">' +
            '        <p class="text-muted small mb-3">Bấm vào một mẫu để xem thử ngay trên menu. Màu được lưu riêng cho tài khoản của bạn trên trình duyệt này.</p>' +
            '        <div class="menu-theme-grid">' + renderCards() + '</div>' +
            '      </div>' +
            '      <div class="modal-footer">' +
            '        <button type="button" class="btn btn-sm btn-light border" data-menu-theme-cancel>Hủy</button>' +
            '        <button type="button" class="btn btn-sm btn-primary px-3" data-menu-theme-save><i class="bi bi-check2 me-1"></i>Lưu</button>' +
            '      </div>' +
            '    </div>' +
            '  </div>' +
            '</div>';
        document.body.insertAdjacentHTML('beforeend', html);
        el = document.getElementById(MODAL_ID);

        // Bấm mẫu -> xem thử ngay
        el.addEventListener('click', function (e) {
            var card = e.target.closest('.menu-theme-card');
            if (card) {
                pendingTheme = card.getAttribute('data-theme');
                apply(pendingTheme);
                markSelected(pendingTheme);
                return;
            }
            if (e.target.closest('[data-menu-theme-cancel]')) {
                apply(savedTheme);               // trả lại màu cũ
                bootstrap.Modal.getOrCreateInstance(el).hide();
                return;
            }
            if (e.target.closest('[data-menu-theme-save]')) {
                var ok = save(pendingTheme);
                savedTheme = pendingTheme;
                bootstrap.Modal.getOrCreateInstance(el).hide();
                if (typeof showToast === 'function') {
                    if (ok) showToast('success', 'Đã lưu màu menu.');
                    else showToast('warning', 'Đã đổi màu nhưng trình duyệt không cho lưu lại (chế độ ẩn danh?).');
                }
            }
        });
        return el;
    }

    function markSelected(id) {
        var cards = document.querySelectorAll('#' + MODAL_ID + ' .menu-theme-card');
        Array.prototype.forEach.call(cards, function (c) {
            var on = c.getAttribute('data-theme') === id;
            c.classList.toggle('selected', on);
            c.setAttribute('aria-pressed', on ? 'true' : 'false');
        });
    }

    /** Mở hộp chọn màu menu (gọi từ menu tài khoản trên topbar). */
    function openPicker() {
        savedTheme = getSaved();
        pendingTheme = savedTheme;
        var el = ensureModal();
        markSelected(savedTheme);
        bootstrap.Modal.getOrCreateInstance(el).show();
    }

    // Đồng bộ lại lần nữa khi script nạp (phòng khi bản lưu không hợp lệ đã bị <head> bỏ qua)
    apply(getSaved());

    return {
        themes: THEMES,
        openPicker: openPicker,
        apply: apply,
        getSaved: getSaved
    };
})();
