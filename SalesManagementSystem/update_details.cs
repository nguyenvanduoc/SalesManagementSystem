using System;
using System.IO;
using System.Text.RegularExpressions;

class Program
{
    static void Main()
    {
        string[] files = {
            @"Views\ChungTuBanHang\_DetailInline.cshtml",
            @"Views\DonDatHang\_DetailInline.cshtml",
            @"Views\PhieuChi\_DetailInline.cshtml",
            @"Views\PhieuNhapKho\_DetailInline.cshtml",
            @"Views\PhieuXuatKho\_DetailInline.cshtml"
        };

        foreach (var file in files)
        {
            if (!File.Exists(file)) continue;
            
            string content = File.ReadAllText(file);
            
            // Fix PhieuChi table to have an ID if it doesn't
            if (file.Contains("PhieuChi"))
            {
                if (content.Contains("<table class=\"table table-sm"))
                {
                    content = content.Replace("<table class=\"table table-sm", "<table id=\"tblDetailInline_@(Model.ID)\" class=\"table table-sm");
                }
                
                // If it doesn't have the style/script, add it
                if (!content.Contains("<style>"))
                {
                    string script = @"
<style>
    #tblDetailInline_@(Model.ID) th {
        position: relative;
        background-clip: padding-box;
    }
    #tblDetailInline_@(Model.ID) th .resizer {
        position: absolute;
        top: 0;
        right: -3px;
        width: 6px;
        height: 100%;
        cursor: col-resize;
        user-select: none;
        z-index: 10;
        background-color: transparent;
    }
    #tblDetailInline_@(Model.ID) th .resizer:hover, #tblDetailInline_@(Model.ID) th .resizer.resizing {
        background-color: rgba(11, 91, 132, 0.5);
    }
</style>
<script>
    (function() {
        var table = document.getElementById('tblDetailInline_@(Model.ID)');
        if (!table) return;
        
        var ths = table.querySelectorAll('th');
        ths.forEach(function(th) {
            if(th.innerText.trim() === '' && (th.style.width === '35px' || th.style.width === '40px' || th.style.width === '50px')) return;
            if(th.innerText.trim() === 'Thao tác' || th.innerText.trim() === 'STT') return;
            
            if(th.querySelector('.resizer')) return;

            var resizer = document.createElement('div');
            resizer.classList.add('resizer');
            th.appendChild(resizer);

            var startX, startWidth;
            
            resizer.addEventListener('mousedown', function(e) {
                startX = e.pageX;
                startWidth = th.offsetWidth;
                resizer.classList.add('resizing');

                var doDrag = function(e) {
                    var newWidth = startWidth + (e.pageX - startX);
                    if(newWidth > 30) {
                        th.style.width = newWidth + 'px';
                        th.style.minWidth = newWidth + 'px';
                        th.style.maxWidth = newWidth + 'px';
                    }
                };

                var stopDrag = function(e) {
                    resizer.classList.remove('resizing');
                    document.removeEventListener('mousemove', doDrag);
                    document.removeEventListener('mouseup', stopDrag);
                };

                document.addEventListener('mousemove', doDrag);
                document.addEventListener('mouseup', stopDrag);
                
                e.preventDefault();
            });
        });
    })();
</script>
";
                    content += script;
                }
            }
            else
            {
                // For other files, replace tblDetailInline with tblDetailInline_@(Model.ID)
                content = content.Replace("tblDetailInline", "tblDetailInline_@(Model.ID)");
                // Fix if we accidentally made it tblDetailInline_@(Model.ID)_@(Model.ID)
                content = content.Replace("tblDetailInline_@(Model.ID)_@(Model.ID)", "tblDetailInline_@(Model.ID)");
            }
            
            File.WriteAllText(file, content);
            Console.WriteLine("Updated " + file);
        }
    }
}
