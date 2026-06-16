import os
import re

count = 0
for root, dirs, files in os.walk('.'):
    dirs[:] = [d for d in dirs if d not in ('bin', 'obj', '.git', 'node_modules')]
    for file in files:
        if file.endswith('.cs') or file.endswith('.cshtml'):
            path = os.path.join(root, file)
            with open(path, 'r', encoding='utf-8', errors='ignore') as f:
                content = f.read()
            
            if 'Ä' in content or 'Ã' in content or 'á' in content or 'Æ' in content:
                def repl(m):
                    s = m.group(1)
                    if 'Ä' in s or 'Ã' in s or 'á' in s or 'Æ' in s:
                        try:
                            decoded = s.encode('cp1252').decode('utf-8')
                            if '\ufffd' not in decoded:
                                return '"' + decoded + '"'
                        except:
                            pass
                    return m.group(0)
                
                new_content = re.sub(r'"([^"\\]*(?:\\.[^"\\]*)*)"', repl, content)
                
                if file.endswith('.cshtml'):
                    def repl_html(m):
                        s = m.group(1)
                        if 'Ä' in s or 'Ã' in s or 'á' in s or 'Æ' in s:
                            try:
                                decoded = s.encode('cp1252').decode('utf-8')
                                if '\ufffd' not in decoded:
                                    return '>' + decoded + '<'
                            except:
                                pass
                        return m.group(0)
                    new_content = re.sub(r'>([^<]+)<', repl_html, new_content)
                
                if new_content != content:
                    with open(path, 'w', encoding='utf-8') as f:
                        f.write(new_content)
                    print('Fixed', path)
                    count += 1
print('Total fixed:', count)
