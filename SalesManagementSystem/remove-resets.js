const fs = require('fs');
const path = require('path');

function walkDir(dir, callback) {
    fs.readdirSync(dir).forEach(f => {
        let dirPath = path.join(dir, f);
        let isDirectory = fs.statSync(dirPath).isDirectory();
        isDirectory ? walkDir(dirPath, callback) : callback(path.join(dir, f));
    });
}

const viewsDir = path.join(__dirname, 'Views');

walkDir(viewsDir, function(filePath) {
    if (!filePath.endsWith('.cshtml')) return;
    
    let content = fs.readFileSync(filePath, 'utf8');
    let lines = content.split(/\r?\n/);
    let newLines = [];
    let inBlock = false;
    let bracketDepth = 0;
    let changed = false;
    
    for (let i = 0; i < lines.length; i++) {
        let line = lines[i];
        
        if (inBlock) {
            bracketDepth += (line.match(/\{/g) || []).length;
            bracketDepth -= (line.match(/\}/g) || []).length;
            if (bracketDepth <= 0) {
                inBlock = false;
            }
            continue;
        }
        
        if (line.match(/\.on\s*\(\s*['"]click.*?\.btn-reset/) || (line.match(/\.click\s*\(\s*function/) && line.includes('btnReset'))) {
            inBlock = true;
            bracketDepth = (line.match(/\{/g) || []).length - (line.match(/\}/g) || []).length;
            if (bracketDepth <= 0) {
                inBlock = false; // single line
            }
            
            // Remove previous .off line if present
            if (newLines.length > 0 && newLines[newLines.length - 1].match(/\.off\s*\(\s*['"]click.*?\.btn-reset/)) {
                newLines.pop();
            }
            
            changed = true;
            continue;
        }
        
        newLines.push(line);
    }
    
    if (changed) {
        fs.writeFileSync(filePath, newLines.join('\r\n'), 'utf8');
        console.log('Cleaned ' + path.basename(filePath));
    }
});
