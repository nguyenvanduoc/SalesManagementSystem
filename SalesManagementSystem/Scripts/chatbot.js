// Trợ lý ảo — widget chat nổi góc phải màn hình
(function ($) {
    'use strict';

    var MAX_HISTORY = 40;

    $(function () {
        var $panel = $('#chatbotPanel');
        if ($panel.length === 0) return;

        var $toggle = $('#chatbotToggle');
        var $messages = $('#chatbotMessages');
        var $form = $('#chatbotForm');
        var $input = $('#chatbotInput');
        var $send = $('#chatbotSend');
        var askUrl = $panel.data('ask-url');
        var storageKey = 'chatbotHistory_' + $panel.data('user');

        var history = loadHistory();
        var lastIntent = '';
        var busy = false;

        // ─── Lưu hội thoại theo phiên trình duyệt (không bắt buộc) ─────────
        function loadHistory() {
            try {
                var raw = sessionStorage.getItem(storageKey);
                return raw ? JSON.parse(raw) : [];
            } catch (e) {
                return [];
            }
        }

        function saveHistory() {
            try {
                sessionStorage.setItem(storageKey, JSON.stringify(history.slice(-MAX_HISTORY)));
            } catch (e) { /* bỏ qua khi trình duyệt chặn storage */ }
        }

        function escapeHtml(s) {
            return $('<div/>').text(s == null ? '' : String(s)).html();
        }

        // ─── Hiển thị ──────────────────────────────────────────────────────
        function scrollToBottom() {
            $messages.scrollTop($messages[0].scrollHeight);
        }

        function appendUser(text) {
            $messages.append('<div class="chatbot-msg user"><div class="chatbot-bubble">' + escapeHtml(text) + '</div></div>');
        }

        function appendError(text) {
            $messages.append('<div class="chatbot-msg bot error"><div class="chatbot-bubble">' + escapeHtml(text) + '</div></div>');
        }

        function buildTable(tbl) {
            var html = '';
            if (tbl.Title) html += '<div class="chatbot-table-title">' + escapeHtml(tbl.Title) + '</div>';
            html += '<div class="chatbot-table-wrap"><table class="chatbot-table"><thead><tr>';
            $.each(tbl.Headers || [], function (i, h) {
                html += '<th>' + escapeHtml(h) + '</th>';
            });
            html += '</tr></thead><tbody>';
            $.each(tbl.Rows || [], function (_, row) {
                html += '<tr>';
                $.each(row, function (i, cell) {
                    var right = tbl.AlignRight && tbl.AlignRight[i];
                    html += '<td' + (right ? ' class="num"' : '') + '>' + escapeHtml(cell) + '</td>';
                });
                html += '</tr>';
            });
            return html + '</tbody></table></div>';
        }

        function appendBot(data) {
            var html = '<div class="chatbot-msg bot"><div class="chatbot-bubble">' + escapeHtml(data.Text);

            $.each(data.Tables || [], function (_, tbl) {
                html += buildTable(tbl);
            });

            if (data.Note) html += '<div class="chatbot-note">' + escapeHtml(data.Note) + '</div>';

            if (data.Links && data.Links.length) {
                html += '<div class="chatbot-links">';
                $.each(data.Links, function (_, link) {
                    html += '<a href="' + escapeHtml(link.Url) + '" class="no-ajax chatbot-link" data-key="' + escapeHtml(link.Key) +
                        '" data-title="' + escapeHtml(link.Title) + '"><i class="bi bi-box-arrow-up-right me-1"></i>Mở ' + escapeHtml(link.Title) + '</a>';
                });
                html += '</div>';
            }
            html += '</div></div>';
            $messages.append(html);
        }

        function renderSuggestions(list) {
            $messages.find('.chatbot-suggestions').remove();
            if (!list || !list.length) return;
            var html = '<div class="chatbot-suggestions">';
            $.each(list, function (_, s) {
                html += '<button type="button" class="chatbot-chip">' + escapeHtml(s) + '</button>';
            });
            $messages.append(html + '</div>');
        }

        function renderHistory() {
            $messages.empty();
            var lastBot = null;
            $.each(history, function (_, item) {
                if (item.role === 'user') appendUser(item.text);
                else if (item.role === 'error') appendError(item.text);
                else { appendBot(item.data); lastBot = item.data; }
            });
            if (lastBot) {
                renderSuggestions(lastBot.Suggestions);
                lastIntent = lastBot.Intent || '';
            }
            scrollToBottom();
        }

        function showTyping() {
            $messages.append('<div class="chatbot-msg bot chatbot-typing-row"><div class="chatbot-bubble chatbot-typing"><span></span><span></span><span></span></div></div>');
            scrollToBottom();
        }

        function setBusy(value) {
            busy = value;
            $send.prop('disabled', value);
            if (!value) $messages.find('.chatbot-typing-row').remove();
        }

        // ─── Gửi câu hỏi ───────────────────────────────────────────────────
        function ask(text, silent) {
            text = $.trim(text || '');
            if (!text || busy) return;

            $messages.find('.chatbot-suggestions').remove();
            if (!silent) {
                appendUser(text);
                history.push({ role: 'user', text: text });
            }
            setBusy(true);
            showTyping();

            $.ajax({
                url: askUrl,
                type: 'POST',
                dataType: 'json',
                global: false,
                data: {
                    message: text,
                    intent: lastIntent,
                    __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').first().val()
                }
            }).done(function (res) {
                setBusy(false);
                if (res && res.success) {
                    appendBot(res.data);
                    renderSuggestions(res.data.Suggestions);
                    lastIntent = res.data.Intent || '';
                    history.push({ role: 'bot', data: res.data });
                } else {
                    var msg = (res && res.message) || 'Không nhận được phản hồi từ trợ lý ảo.';
                    appendError(msg);
                    history.push({ role: 'error', text: msg });
                }
                saveHistory();
                scrollToBottom();
            }).fail(function (xhr) {
                setBusy(false);
                var msg = xhr.status === 401
                    ? 'Phiên đăng nhập đã hết hạn, vui lòng đăng nhập lại.'
                    : 'Không kết nối được tới máy chủ, vui lòng thử lại.';
                appendError(msg);
                scrollToBottom();
            });
        }

        // ─── Đóng / mở ─────────────────────────────────────────────────────
        function openPanel() {
            $panel.addClass('is-open').attr('aria-hidden', 'false');
            $toggle.addClass('is-open');
            if (history.length === 0 && $messages.children().length === 0) ask('xin chào', true);
            setTimeout(function () { $input.trigger('focus'); }, 150);
            scrollToBottom();
        }

        function closePanel() {
            $panel.removeClass('is-open').attr('aria-hidden', 'true');
            $toggle.removeClass('is-open');
        }

        $toggle.on('click', openPanel);
        $('#chatbotClose').on('click', closePanel);

        $('#chatbotClear').on('click', function () {
            history = [];
            lastIntent = '';
            saveHistory();
            $messages.empty();
            ask('xin chào', true);
        });

        $(document).on('keydown', function (e) {
            if (e.key === 'Escape' && $panel.hasClass('is-open') && $('.modal.show').length === 0) closePanel();
        });

        $form.on('submit', function (e) {
            e.preventDefault();
            var text = $input.val();
            $input.val('');
            ask(text, false);
        });

        $messages.on('click', '.chatbot-chip', function () {
            ask($(this).text(), false);
        });

        // Mở màn hình chi tiết trong tab của hệ thống (giống tìm kiếm menu)
        $messages.on('click', '.chatbot-link', function (e) {
            if (typeof TabManager !== 'undefined' && TabManager.openTab) {
                e.preventDefault();
                TabManager.openTab($(this).data('key'), $(this).data('title'), $(this).attr('href'));
            }
        });

        renderHistory();
    });
})(jQuery);
