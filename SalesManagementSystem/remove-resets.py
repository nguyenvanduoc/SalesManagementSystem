import os
import re

directory = r'C:\Users\duoc0\OneDrive\Desktop\WEB_QLBH\QuanLyBanHang\SalesManagementSystem\SalesManagementSystem\Views'

def process_file(filepath):
    with open(filepath, 'r', encoding='utf-8') as f:
        lines = f.readlines()
        
    new_lines = []
    in_block = False
    bracket_depth = 0
    changed = False
    
    for i, line in enumerate(lines):
        if in_block:
            bracket_depth += line.count('{')
            bracket_depth -= line.count('}')
            if bracket_depth <= 0:
                in_block = False
            continue
            
        if re.search(r'\.on\s*\(\s*[\''"]click.*?\.btn-reset', line) or (re.search(r'\.click\s*\(\s*function', line) and 'btnReset' in line):
            in_block = True
            bracket_depth = line.count('{') - line.count('}')
            if bracket_depth <= 0:
                in_block = False
            
            # Remove previous .off line if present
            if len(new_lines) > 0 and re.search(r'\.off\s*\(\s*[\''"]click.*?\.btn-reset', new_lines[-1]):
                new_lines.pop()
            
            changed = True
            continue
            
        new_lines.append(line)
        
    if changed:
        with open(filepath, 'w', encoding='utf-8') as f:
            f.writelines(new_lines)
        print(f"Cleaned {os.path.basename(filepath)}")

for root, _, files in os.walk(directory):
    for file in files:
        if file.endswith('.cshtml'):
            process_file(os.path.join(root, file))
