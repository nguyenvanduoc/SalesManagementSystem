using System;
using System.IO;

class Program {
    static void Main() {
        string path = @"c:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Scripts\tab-manager.js";
        string content = File.ReadAllText(path);
        
        int start = content.IndexOf("$(document).on('hide.bs.tab'");
        if (start == -1) start = content.IndexOf("$(document).on('show.bs.tab'");
        
        int end = content.IndexOf("$(document).on('shown.bs.tab'");
        
        if (start != -1 && end != -1) {
            string code = @"
        // Qu?n lý Modal caching d? không b? dè n?i dung khi m? ? nhi?u tab
        $(document).on('hide.bs.tab', 'button[data-bs-toggle=""tab""]', function (e) {
            var previousTabId = $(e.target).attr('data-bs-target');
            if (previousTabId) previousTabId = previousTabId.replace('#', '');
            
            var modalOwner = $('#globalFormModal').attr('data-owner-tab');
            if (modalOwner && modalOwner === previousTabId) {
                var $cache = $('#' + previousTabId).find('.modal-cache');
                if ($cache.length === 0) {
                    $cache = $('<div class=""modal-cache d-none""></div>').appendTo('#' + previousTabId);
                }
                $cache.append($('#globalFormModalContent').children());
                var isVisible = !$('#globalFormModal').hasClass('d-none') && $('#globalFormModal').is(':visible');
                $('#' + previousTabId).data('modal-visible', isVisible);
            }
        });

        // X? lý Modal khi chuy?n Tab
        $(document).on('show.bs.tab', 'button[data-bs-toggle=""tab""]', function (e) {
            var targetTabId = $(e.target).attr('data-bs-target');
            if (targetTabId) targetTabId = targetTabId.replace('#', '');
            
            // Xóa r?ng global modal content (vì d? li?u dã du?c d?y vào cache c?a tab cu)
            $('#globalFormModalContent').empty();
            
            var $cache = $('#' + targetTabId).find('.modal-cache');
            if ($cache.length > 0 && $cache.children().length > 0) {
                $('#globalFormModalContent').append($cache.children());
                $('#globalFormModal').attr('data-owner-tab', targetTabId);
                
                if ($('#' + targetTabId).data('modal-visible')) {
                    $('#globalFormModal').removeClass('d-none');
                    $('.modal-backdrop').removeClass('d-none');
                } else {
                    $('#globalFormModal').addClass('d-none');
                    $('.modal-backdrop').addClass('d-none');
                }
            } else {
                $('#globalFormModal').removeAttr('data-owner-tab');
                $('#globalFormModal').addClass('d-none');
                $('.modal-backdrop').addClass('d-none');
            }
        });

        ";
            string newContent = content.Substring(0, start) + code + content.Substring(end);
            File.WriteAllText(path, newContent);
            Console.WriteLine("SUCCESS");
        } else {
            Console.WriteLine("NOT FOUND");
        }
    }
}
