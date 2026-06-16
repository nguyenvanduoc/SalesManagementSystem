const fs = require('fs');
const path = require('path');
const iconv = require('iconv-lite');

function getFiles(dir, files_) {
    files_ = files_ || [];
    const files = fs.readdirSync(dir);
    for (let i in files) {
        const name = dir + '/' + files[i];
        if (fs.statSync(name).isDirectory()) {
            if (!name.includes('node_modules') && !name.includes('.git') && !name.includes('obj') && !name.includes('bin')) {
                getFiles(name, files_);
            }
        } else {
            if (name.endsWith('.cs') || name.endsWith('.cshtml')) {
                files_.push(name);
            }
        }
    }
    return files_;
}

const allFiles = getFiles('.');
let totalFixed = 0;

for (const file of allFiles) {
    let originalText = fs.readFileSync(file, 'utf8');
    
    // Quick check if file has suspicious characters
    if (!originalText.includes('Ã') && !originalText.includes('Ä') && !originalText.includes('á') && !originalText.includes('Æ')) continue;

    // Pattern matches sequences of characters that were likely misinterpreted as Windows-1252.
    // In Node.js, we can match block by block.
    // Instead of regex, let's just find strings between quotes and fix them.
    let fixedText = originalText.replace(/"([^"\\]*(\\.[^"\\]*)*)"/g, (match, p1) => {
        if (p1.includes('Ã') || p1.includes('Ä') || p1.includes('á') || p1.includes('Æ')) {
            // Attempt to decode the string
            // 1. encode to win1252 (this gives the original utf8 bytes)
            const buf = iconv.encode(p1, 'win1252');
            
            // 2. decode as utf8
            const decoded = iconv.decode(buf, 'utf8');
            
            // 3. check if it contains replacement char (which means invalid utf8)
            if (!decoded.includes('')) {
                // Return reconstructed quote
                return '"' + decoded + '"';
            }
        }
        return match; // return original if failed
    });
    
    // Also try to replace HTML text (not in quotes) in .cshtml files
    if (file.endsWith('.cshtml')) {
        fixedText = fixedText.replace(/>([^<]+)</g, (match, p1) => {
            if (p1.includes('Ã') || p1.includes('Ä') || p1.includes('á') || p1.includes('Æ')) {
                const buf = iconv.encode(p1, 'win1252');
                const decoded = iconv.decode(buf, 'utf8');
                if (!decoded.includes('')) {
                    return '>' + decoded + '<';
                }
            }
            return match;
        });
        
        // Also comments
        fixedText = fixedText.replace(/\/\/([^\n]+)/g, (match, p1) => {
            if (p1.includes('Ã') || p1.includes('Ä') || p1.includes('á') || p1.includes('Æ')) {
                const buf = iconv.encode(p1, 'win1252');
                const decoded = iconv.decode(buf, 'utf8');
                if (!decoded.includes('')) {
                    return '//' + decoded;
                }
            }
            return match;
        });
    } else {
        // Also fix comments in .cs files
        fixedText = fixedText.replace(/\/\/([^\n]+)/g, (match, p1) => {
            if (p1.includes('Ã') || p1.includes('Ä') || p1.includes('á') || p1.includes('Æ')) {
                const buf = iconv.encode(p1, 'win1252');
                const decoded = iconv.decode(buf, 'utf8');
                if (!decoded.includes('')) {
                    return '//' + decoded;
                }
            }
            return match;
        });
    }

    if (originalText !== fixedText) {
        fs.writeFileSync(file, fixedText, 'utf8');
        console.log('Fixed ' + file);
        totalFixed++;
    }
}
console.log('Total fixed: ' + totalFixed);
