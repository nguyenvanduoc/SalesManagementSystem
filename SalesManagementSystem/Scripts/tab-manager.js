/**
 * SPA Tab Manager for SalesManagementSystem
 *
 * FIX: Ngăn chặn việc đăng ký sự kiện ajax-link và pageSizeSelect trùng lặp
 * khi nhiều tab được mở cùng lúc, dẫn đến delay và sai dữ liệu phân trang.
 *
 * Giải pháp: Tập trung xử lý phân trang (.ajax-link, #pageSizeSelect, searchForm)
 * ngay tại tab-manager.js, dùng namespace event để có thể off() khi cần.
 * Mỗi handler luôn scope vào đúng tab-pane chứa phần tử, không dùng selector
 * toàn cục như $('#gridData') hay $('#table-container').
 */
var TabManager = (function () {
    var MAX_TABS = 20;

    // ─── Helpers ────────────────────────────────────────────────────────────

    /**
     * Tìm container dữ liệu dạng lưới trong một tab-pane.
     * Sử dụng attribute selector để tránh lỗi document.getElementById khi có nhiều tab
     * dùng chung id="gridData" hoặc id="table-container".
     */
    function findGrid(pane) {
        var grid = pane.find('[id="gridData"], [id="table-container"], .table-responsive').first();
        // Nếu dùng .table-responsive, ta lùi ra cha của nó (ví dụ card-body) nếu cần,
        // nhưng tốt nhất là lấy chính xác [id="..."]
        var exactId = pane.find('[id="gridData"], [id="table-container"]').first();
        if (exactId.length > 0) return exactId;

        // Fallback vào table-container cha
        return grid.length > 0 ? grid.parent() : null;
    }

    var activeRequests = {};

    function showLoadingLocal($container) {
        if (!$container || $container.length === 0) return;
        if ($container.css('position') === 'static') {
            $container.css('position', 'relative');
        }
        
        $container.find('.tab-loading-overlay').remove();
        var overlay = $('<div>', { class: 'tab-loading-overlay' });
        overlay.html('<div class="spinner-border text-primary" role="status"><span class="visually-hidden">Loading...</span></div>');
        $container.append(overlay);
    }

    function hideLoadingLocal($container) {
        if (!$container || $container.length === 0) return;
        $container.find('.tab-loading-overlay').remove();
    }

    function ajaxLoadGrid(url, $container) {
        var containerId = $container.attr('id') || 'grid-' + Math.random().toString(36).substr(2, 9);
        $container.attr('id', containerId);

        if (activeRequests[containerId]) {
            activeRequests[containerId].abort();
        }

        showLoadingLocal($container);

        activeRequests[containerId] = $.ajax({
            url: url,
            type: 'GET',
            headers: {
                'X-Requested-With': 'XMLHttpRequest'
            },
            success: function (result) {
                hideLoadingLocal($container);
                delete activeRequests[containerId];
                // Kiểm tra nếu server trả về trang đăng nhập (session hết hạn)
                if (typeof result === 'string' && result.indexOf('<form') > -1 && result.indexOf('login') > -1) {
                    if (typeof showToast === 'function') {
                        showToast('warning', 'Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.');
                    }
                    return;
                }
                $container.html(result);
            },
            error: function (xhr, status) {
                if (status === 'abort') return;
                hideLoadingLocal($container);
                delete activeRequests[containerId];
                var msg = 'Lỗi tải dữ liệu (HTTP ' + xhr.status + ')';
                if (xhr.status === 403) msg = 'Bạn không có quyền xem dữ liệu này.';
                else if (xhr.status === 401) msg = 'Phiên đăng nhập đã hết hạn.';
                else if (xhr.status === 404) msg = 'Không tìm thấy dữ liệu.';
                if (typeof showToast === 'function') {
                    showToast('error', msg);
                } else {
                    console.error('[TabManager] ajaxLoadGrid error:', xhr.status, url);
                }
            }
        });
    }

    // ─── Intercept tập trung: .ajax-link (phân trang) ───────────────────────

    /**
     * Bắt click trên các nút phân trang (.ajax-link) bên trong bất kỳ tab-pane nào.
     * Dùng namespace 'tabmanager' để không xung đột với handler cũ ở từng View.
     * stopImmediatePropagation() để chặn handler trùng đăng ký ở View chạy tiếp.
     */
    function initPaginationHandler() {
        $(document).off('click.tabmanager', '.ajax-link');
        $(document).on('click.tabmanager', '.ajax-link', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();

            var $li = $(this).closest('li');
            if ($li.hasClass('disabled') || $li.hasClass('active')) return;

            var url = $(this).attr('href');
            if (!url || url === '#' || url.indexOf('javascript:') === 0) return;

            // Scope tìm container vào đúng tab-pane chứa link này
            var $pane = $(this).closest('.tab-pane');
            var grid = null;
            if ($pane.length > 0) {
                grid = findGrid($pane);
            }

            if (grid) {
                ajaxLoadGrid(url, grid);
            } else {
                // Fallback: nếu không nằm trong tab-pane (ví dụ chạy độc lập không qua SPA)
                var $fallback = $('[id="table-container"], [id="gridData"]').last();
                if ($fallback.length > 0) {
                    ajaxLoadGrid(url, $fallback);
                } else {
                    console.error("Không tìm thấy container để tải lưới dữ liệu.");
                }
            }
        });
    }

    /**
     * Bắt thay đổi pageSize. Luôn scope vào tab-pane để tránh đụng chéo.
     */
    function initPageSizeHandler() {
        $(document).off('change.tabmanager', '.page-size-select');
        $(document).on('change.tabmanager', '.page-size-select', function (e) {
            e.preventDefault();
            e.stopImmediatePropagation();

            var pageSize = $(this).val();
            var $pane = $(this).closest('.tab-pane');

            // Lấy keyword từ form tìm kiếm trong cùng pane (nếu có)
            var keyword = $pane.find('input[name="keyword"]').val() || '';

            // Tìm URL gốc từ data-action của thẻ cha
            var $paginationBlock = $(this).closest('.custom-pagination');
            var baseUrl = $paginationBlock.attr('data-action');

            if (!baseUrl) {
                var $anyLink = $paginationBlock.find('.ajax-link').first();
                if ($anyLink.length > 0) {
                    baseUrl = $anyLink.attr('href').split('?')[0];
                }
            }

            if (!baseUrl) return;

            var url = baseUrl + '?page=1&pageSize=' + pageSize;
            if (keyword) {
                url += '&keyword=' + encodeURIComponent(keyword);
            }

            var grid = null;
            if ($pane.length > 0) {
                grid = findGrid($pane);
            }

            if (grid) {
                ajaxLoadGrid(url, grid);
            } else {
                var $fallback = $('[id="table-container"], [id="gridData"]').last();
                if ($fallback.length > 0) {
                    ajaxLoadGrid(url, $fallback);
                }
            }
        });
    }

    // ─── Intercept tập trung: form tìm kiếm (#searchForm) ──────────────────

    /**
     * Bắt submit form tìm kiếm trong tab-pane (KHÔNG bắt modal form hoặc các form khác).
     * Để form submit trong modal vẫn chạy qua handler riêng ở _Layout.cshtml.
     *
     * NOTE: handler này chỉ cần khi view KHÔNG có handler form riêng.
     * Nếu view có handler riêng đã e.preventDefault() rồi thì sẽ không ảnh hưởng.
     */
    function initSearchFormHandler() {
        $(document).on('submit.tabmanager', '.tab-pane #searchForm', function (e) {
            // Để handler gốc của từng view tự xử lý nếu nó đã gọi e.preventDefault()
            // — handler gốc chạy trước do được đăng ký sau khi DOM load
            // Chỉ chặn nếu chưa được preventDefault bởi view
            if (e.isDefaultPrevented()) return;

            e.preventDefault();
            var $form = $(this);
            var action = $form.attr('action');
            var data = $form.serialize();
            var url = action + '?' + data;

            var $pane = $form.closest('.tab-pane');
            var grid = findGrid($pane);
            if (grid) {
                ajaxLoadGrid(url, grid);
            }
        });
    }

    // ─── Khởi tạo interceptor tab-pane a (điều hướng nội bộ) ───────────────

    function initTabLinkInterceptor() {
        $(document).on('click', '.tab-pane a', function (e) {
            var href = $(this).attr('href');
            var target = $(this).attr('target');

            // Bỏ qua: rỗng, #, javascript:, _blank, no-ajax, ajax-link (đã xử lý riêng), pagination
            if (!href
                || href === '#'
                || href.indexOf('javascript:') === 0
                || target === '_blank'
                || $(this).hasClass('no-ajax')
                || $(this).hasClass('ajax-link')
                || $(this).closest('.pagination').length > 0) {
                return;
            }

            e.preventDefault();
            var paneId = $(this).closest('.tab-pane').attr('id');
            var key = paneId.replace('-pane', '');

            loadTabContent(key, href);
            $('#' + paneId).attr('data-url', href);
        });
    }

    // ─── Intercept form submit trong tab (trừ globalFormModal) ─────────────

    function initTabFormSubmitInterceptor() {
        $(document).on('submit', '.tab-pane form', function (e) {
            // Không intercept nếu form có thuộc tính data-ajax="false", .no-ajax
            // hoặc đang nằm trong globalFormModal (đã có handler riêng ở _Layout)
            if ($(this).attr('data-ajax') === 'false'
                || $(this).hasClass('no-ajax')
                || $(this).closest('#globalFormModalContent').length > 0
                || $(this).attr('id') === 'globalDeleteForm') {
                return;
            }

            // Nếu là searchForm trong tab-pane: để initSearchFormHandler xử lý
            // (hoặc handler riêng của view đã xử lý)
            if ($(this).attr('id') === 'searchForm') return;

            e.preventDefault();
            var form = $(this);
            var paneId = form.closest('.tab-pane').attr('id');
            var key = paneId.replace('-pane', '');

            var hasFiles = form.find('input[type="file"]').length > 0;
            var url = form.attr('action') || $('#' + paneId).attr('data-url');
            var method = form.attr('method') || 'GET';

            var $pane = $('#' + paneId);
            if (activeRequests[paneId]) {
                activeRequests[paneId].abort();
            }

            showLoadingLocal($pane);

            var ajaxOptions = {
                url: url,
                type: method,
                beforeSend: function (xhr) {
                    xhr.setRequestHeader('X-Requested-With', 'SPA');
                    xhr.setRequestHeader('X-SPA-Load', 'true');
                },
                success: function (res) {
                    hideLoadingLocal($pane);
                    delete activeRequests[paneId];
                    if (typeof res === 'object') {
                        if (res.message && typeof showToast === 'function') {
                            showToast(res.success === false ? 'error' : (res.type || 'success'), res.message);
                        }
                        if (res.closeTab) {
                            closeTab('tab-' + key);
                            setTimeout(function() { reloadActiveTabGrid(false); }, 100);
                        } else if (res.redirectUrl) {
                            loadTabContent(key, res.redirectUrl);
                        }
                    } else {
                        $pane.html(res);
                        reInitPlugins(paneId);
                    }
                },
                error: function (err, status) {
                    if (status === 'abort') return;
                    hideLoadingLocal($pane);
                    delete activeRequests[paneId];
                    console.error('Lỗi submit form:', err);
                    $pane.html('<div class="alert alert-danger">Lỗi kết nối máy chủ. Vui lòng thử lại.</div>');
                }
            };

            if (hasFiles && method.toUpperCase() === 'POST') {
                ajaxOptions.data = new FormData(this);
                ajaxOptions.processData = false;
                ajaxOptions.contentType = false;
            } else {
                ajaxOptions.data = form.serialize();
            }

            activeRequests[paneId] = $.ajax(ajaxOptions);
        });
    }

    // ─── init() ─────────────────────────────────────────────────────────────

    function init() {
        // Phân trang & pageSize — TẬP TRUNG, dùng namespace, luôn scope đúng pane
        initPaginationHandler();
        initPageSizeHandler();
        initSearchFormHandler();

        // Điều hướng link và form bên trong tab
        initTabLinkInterceptor();
        initTabFormSubmitInterceptor();

        // Khởi tạo plugins cho Default Tab
        reInitPlugins('default-tab-pane');

        // Đóng toàn bộ Modal khi người dùng chuyển Tab
        $(document).on('show.bs.tab', 'button[data-bs-toggle="tab"]', function (e) {
            $('.modal').modal('hide');
            $('.modal-backdrop').remove();
            $('body').removeClass('modal-open').css('padding-right', '');
        });

        // Nút cuộn ngang thanh Tab
        $('#btnScrollLeftTab').on('click', function () {
            var container = document.getElementById('tabsContainerScroll');
            if (container) {
                container.scrollBy({ left: -200, behavior: 'smooth' });
            }
        });

        $('#btnScrollRightTab').on('click', function () {
            var container = document.getElementById('tabsContainerScroll');
            if (container) {
                container.scrollBy({ left: 200, behavior: 'smooth' });
            }
        });
    }

    // ─── openTab() ──────────────────────────────────────────────────────────

    function openTab(key, title, url) {
        var tabId = 'tab-' + key;
        var paneId = tabId + '-pane';

        // Nếu tab đã tồn tại thì chỉ switch, không reload
        if ($('#' + tabId).length > 0) {
            setActiveTab(tabId);
            return;
        }

        // Kiểm tra giới hạn số lượng Tab cấu hình tại MAX_TABS
        var currentTabs = $('#mainTabHeader .nav-item').length;
        if (currentTabs >= MAX_TABS) {
            alert('Bạn đã mở tối đa ' + MAX_TABS + ' tab. Vui lòng đóng bớt để tiếp tục.');
            return;
        }

        // Tạo tab header
        var li = $('<li>', { class: 'nav-item', role: 'presentation' });
        var btn = $('<button>', {
            class: 'nav-link',
            id: tabId,
            'data-bs-toggle': 'tab',
            'data-bs-target': '#' + paneId,
            type: 'button',
            role: 'tab',
            'aria-controls': paneId,
            'aria-selected': 'false'
        });

        var titleSpan = $('<span>', { class: 'tab-title', text: title, title: title });
        var closeBtn = $('<span>', { class: 'close-tab-btn', html: '&times;', title: 'Đóng tab' });

        closeBtn.on('click', function (e) {
            e.stopPropagation();
            closeTab(tabId);
        });

        btn.append(titleSpan).append(closeBtn);

        btn.on('click', function (e) {
            if ($(e.target).hasClass('close-tab-btn')) return;
        });

        li.append(btn);
        $('#mainTabHeader').append(li);

        // Tạo tab-pane
        var pane = $('<div>', {
            class: 'tab-pane fade',
            id: paneId,
            role: 'tabpanel',
            'aria-labelledby': tabId,
            tabindex: '0',
            'data-url': url
        });

        $('#mainTabContent').append(pane);

        // Chuyển sang tab mới và hiển thị giao diện TRƯỚC
        setActiveTab(tabId);
        
        // Trì hoãn việc gọi Ajax 50ms để trình duyệt kịp vẽ (paint) Tab và Loading Spinner ra màn hình
        // Tránh tình trạng bị "đơ" không chuyển tab ngay khi request chậm
        setTimeout(function() {
            loadTabContent(tabId, url);
        }, 50);
    }

    // ─── setActiveTab() ─────────────────────────────────────────────────────

    function setActiveTab(tabId) {
        var triggerEl = document.querySelector('#' + tabId);
        if (triggerEl) {
            var tab = bootstrap.Tab.getOrCreateInstance(triggerEl);
            tab.show();

            // Tự động cuộn Tab đang được chọn vào vùng nhìn thấy (nếu bị khuất)
            setTimeout(function() {
                var container = document.getElementById('tabsContainerScroll');
                if (!container) return;
                var li = triggerEl.closest('li');
                if (!li) return;
                
                var containerRect = container.getBoundingClientRect();
                var liRect = li.getBoundingClientRect();
                
                // Trừ hao 30px để nhìn thấy 1 chút của tab kế bên
                if (liRect.left < containerRect.left) {
                    // Khuất bên trái
                    container.scrollBy({ left: liRect.left - containerRect.left - 30, behavior: 'smooth' });
                } else if (liRect.right > containerRect.right) {
                    // Khuất bên phải
                    container.scrollBy({ left: liRect.right - containerRect.right + 30, behavior: 'smooth' });
                }
            }, 50);
        }
    }

    // ─── closeTab() ─────────────────────────────────────────────────────────

    function closeTab(tabId) {
        var paneId = tabId + '-pane';
        var isActive = $('#' + tabId).hasClass('active');

        if (isActive) {
            var prevTab = $('#' + tabId).parent().prev('.nav-item').find('.nav-link');
            if (prevTab.length > 0) {
                setActiveTab(prevTab.attr('id'));
            } else {
                var nextTab = $('#' + tabId).parent().next('.nav-item').find('.nav-link');
                if (nextTab.length > 0) {
                    setActiveTab(nextTab.attr('id'));
                }
            }
        }

        $('#' + tabId).parent().remove();
        $('#' + paneId).remove();
    }

    // ─── loadTabContent() ───────────────────────────────────────────────────

    function loadTabContent(tabId, url) {
        var paneId = tabId + '-pane';
        var $pane = $('#' + paneId);

        if (activeRequests[paneId]) {
            activeRequests[paneId].abort();
        }

        showLoadingLocal($pane);

        activeRequests[paneId] = $.ajax({
            url: url,
            type: 'GET',
            beforeSend: function (xhr) {
                xhr.setRequestHeader('X-Requested-With', 'SPA');
                xhr.setRequestHeader('X-SPA-Load', 'true');
            },
            success: function (res) {
                hideLoadingLocal($pane);
                delete activeRequests[paneId];
                
                $pane.html(res);
                reInitPlugins(paneId);
            },
            error: function (err, status) {
                if (status === 'abort') return;
                
                hideLoadingLocal($pane);
                delete activeRequests[paneId];
                
                console.error('Lỗi nạp nội dung tab:', err);
                $pane.html('<div class="alert alert-danger m-3">Lỗi kết nối máy chủ khi nạp giao diện. Vui lòng thử lại.</div>');
            }
        });
    }

    // Đã gỡ bỏ toàn bộ cơ chế global loading để tránh flash/flicker toàn bộ layout.
    $(document).off('ajaxStart ajaxStop');

    // ─── reInitPlugins() ────────────────────────────────────────────────────

    function reInitPlugins(paneId) {
        var container = $('#' + paneId);

        if ($.fn.select2) {
            container.find('.select2').select2({ width: '100%' });
        }

        var tooltipTriggerList = [].slice.call(container[0].querySelectorAll('[data-bs-toggle="tooltip"]'));
        tooltipTriggerList.map(function (tooltipTriggerEl) {
            return new bootstrap.Tooltip(tooltipTriggerEl);
        });

        $(document).trigger('tabContentLoaded', [paneId]);
    }

    // ─── reloadActiveTabGrid() ──────────────────────────────────────────────

    /**
     * Tải lại dữ liệu grid của tab đang active.
     * Được gọi từ _Layout.cshtml sau khi form save/delete thành công,
     * thay thế việc gọi loadData() toàn cục (dễ bị overwrite khi nhiều tab mở).
     * @param {boolean} resetPage - nếu true, load lại trang 1; nếu false, giữ nguyên trang hiện tại
     */
    function reloadActiveTabGrid(resetPage) {
        var $activePane = $('.tab-pane.active.show');
        if ($activePane.length === 0) {
            $activePane = $('.tab-pane.active');
        }
        if ($activePane.length === 0) return;

        var grid = findGrid($activePane);
        if (!grid) {
            // Fallback nếu không tìm thấy grid
            if (typeof loadData === 'function') { loadData(''); }
            return;
        }

        // Ưu tiên dùng link trang hiện tại (active page link) để giữ nguyên vị trí
        var $activePagLink = $activePane.find('.pagination .page-item.active .ajax-link').first();
        // Fallback về link đầu tiên (sẽ là trang 1)
        var $firstPagLink = $activePane.find('.ajax-link').first();

        var url = null;
        if (!resetPage && $activePagLink.length > 0) {
            url = $activePagLink.attr('href');
        } else if ($firstPagLink.length > 0) {
            // Reset về trang 1
            try {
                var baseUrl = $firstPagLink.attr('href');
                var urlObj = new URL(baseUrl, window.location.origin);
                urlObj.searchParams.set('page', '1');
                url = urlObj.pathname + urlObj.search;
            } catch (ex) {
                url = $firstPagLink.attr('href');
            }
        }

        if (url) {
            ajaxLoadGrid(url, grid);
        } else if (typeof loadData === 'function') {
            loadData('');
        }
    }

    // ─── Public API ─────────────────────────────────────────────────────────

    return {
        init: init,
        openTab: openTab,
        closeTab: closeTab,
        setActiveTab: setActiveTab,
        reloadActiveTabGrid: reloadActiveTabGrid
    };
})();

$(document).ready(function () {
    TabManager.init();
});
